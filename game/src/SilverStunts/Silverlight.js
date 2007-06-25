///////////////////////////////////////////////////////////////////////////////
//
//  Silverlight.js   version 0.9
//
//  This file is provided by Microsoft as a helper file for websites that
//  incorporate Silverlight Objects.  It must be used in conjunction with createSilverlight.js, 
//  or alternatively, a custom .js file specific to your site.  The 0.9 version of this file is 
//  hard coded to match Microsoft Silverlight v1.0 Beta, which exposes 0.9 as its version number.     
//  This file is provided as is.
//
///////////////////////////////////////////////////////////////////////////////
if (!window.Sys) 
{
   window.Sys = { };
}
if (!window.Sys.Silverlight) 
{
    window.Sys.Silverlight = { };
}
//////////////////////////////////////////////////////////////////
// isInstalled, checks to see if the correct version is installed
//////////////////////////////////////////////////////////////////
Sys.Silverlight.isInstalled = function(version)
{
 
    var uaString = navigator.userAgent;
    var reqVersionArray = version.split(".");
    reqMajorVer = (reqVersionArray[0] != null) ? reqVersionArray[0] : 0;
    reqMinorVer = (reqVersionArray[1] != null) ? reqVersionArray[1] : 9;
    reqBuildVer = (reqVersionArray[2] != null) ? reqVersionArray[2] : 0;
      
    
    function detectAgControlVersion()
    {
        agVersion = -1;       

        if ((navigator.plugins != null) && (navigator.plugins.length > 0))
        {        
		if (document.getElementById && !document.all && navigator.plugins["WPFe Plug-In"] )
		{
			if (navigator.userAgent.indexOf("Firefox") != -1)
			{
				var tmpAgObjectTag = '<object id="tmpSilverlightVersion" width="1" height="1" type="application/ag-plugin"/>';
				range = document.createRange();	
				range.selectNode(document.body);			
				range.setStartBefore(document.body);
				tmpAgControlDiv = document.createElement('DIV');
				document.body.appendChild(tmpAgControlDiv);
				tmpAgControlDiv.innerHTML=tmpAgObjectTag;
				agVersionElement=document.getElementById("tmpSilverlightVersion");
				agVersion=agVersionElement.settings.version;
				document.body.removeChild(tmpAgControlDiv);
			}
			else
			{
				agVersion = navigator.plugins["WPFe Plug-In"].description;
			}
		}
        }
        else if ((navigator.userAgent.indexOf('Windows') != -1) && (navigator.appVersion.indexOf('MSIE') != -1) )
        {
            try
            {
                var AgControl = new ActiveXObject("AgControl.AgControl");            
                agVersion = AgControl.settings.version;                
                AgControl = null;
                
            }
            catch (e)
            {
                agVersion = -1;
            }
        }
        return agVersion;
    }
    var versionStr = detectAgControlVersion();
    if (versionStr == -1 )
    {
        return false;
    }
    else if (versionStr != 0)
    {
        versionArray = versionStr.split(".");

        var versionMajor = versionArray[0];
        var versionMinor = versionArray[1];
        var versionBuild = versionArray[2];

        if (versionMajor > parseFloat(reqMajorVer))
        {
            return true;
        }
        else if (versionMajor == parseFloat(reqMajorVer))
        {
            if (versionMinor > parseFloat(reqMinorVer))
            {
                return true;
            }
            else if (versionMinor == parseFloat(reqMinorVer))
            {
                if (versionBuild >= parseFloat(reqBuildVer))
                {
                    return true;
                }
            }
        }
        return false;
    }
}
// Silverlight event instance counter for memory mgt
Sys.Silverlight._counterL = 0;

///////////////////////////////////////////////////////////////////////////////
// createObject();  Params:
// parentElement of type Element, the parent element of the Silverlight Control
// source of type String
// id of type string
// properties of type String, object literal notation { name:value, name:value, name:value}, 
//     current properties are: width, height, background, framerate, isWindowless, enableHtmlAccess, inplaceInstallPrompt:  all are of type string
// events of type String, object literal notation { name:value, name:value, name:value}, 
//     current events are onLoad onError, both are type string
// initParams of type Object or object literal notation { name:value, name:value, name:value}
// userContext of type Object 
/////////////////////////////////////////////////////////////////////////////////

Sys.Silverlight.createObject = function(source, parentElement, id, properties, events, initParams, userContext)
{
        
    var slPluginHelper = new Object();
    var slProperties = properties;        
    var slEvents = events;
    slPluginHelper.source = source;
    slPluginHelper.parentElement = parentElement;
    slPluginHelper.id = id;         
    slPluginHelper.width = slProperties.width;
    slPluginHelper.height = slProperties.height;
    slPluginHelper.background = slProperties.background;        
    slPluginHelper.isWindowless = slProperties.isWindowless;
    slPluginHelper.framerate = slProperties.framerate;
    slPluginHelper.ignoreBrowserVer = slProperties.ignoreBrowserVer;    
    slPluginHelper.inplaceInstallPrompt = slProperties.inplaceInstallPrompt;
    slPluginHelper.enableHtmlAccess = slProperties.enableHtmlAccess;
    slPluginHelper.initParams = initParams;        
    
    //memory management for onLoad event      
    if (slEvents.onLoad) 
    {
       var uniqueID = '_sl' + (Sys.Silverlight._counterL++);
       slPluginHelper.loadedHandlerName = 'javascript:' + uniqueID;  

       function _dispose()
       {
        if (window.detachEvent) 
        {
            window.detachEvent('onunload', _dispose);
        }
        else 
        {
            window.removeEventListener('unload', _dispose, false);
        }
        window[uniqueID] = null;
       }

        function _loadedHandler(sender) 
        {
            slEvents.onLoad(document.getElementById(slPluginHelper.id), userContext, sender); 
            _dispose();
        }

        window[uniqueID] = _loadedHandler;  
        if (window.attachEvent) 
        {
            window.attachEvent('onunload', _dispose);
        }
        else 
        {
            window.addEventListener('unload', _dispose, false);
        }
    }
    //set error handler
    if (!slEvents.onError)
    {
        slPluginHelper.onError = "default_error_handler";
    }
    else
    {
        slPluginHelper.onError = slEvents.onError;
    }          
    
    var slPluginHTML = "";
        
    //direct download pointer
    var directDownload;

    if (navigator.userAgent.indexOf('Windows') != -1)
    {
        directDownload = "http://go.microsoft.com/fwlink/?LinkID=86008";
    }

    else if (navigator.userAgent.indexOf('PPC Mac OS X') != -1)
    {
        directDownload = "http://go.microsoft.com/fwlink/?LinkID=87380";
    }

    else if (navigator.userAgent.indexOf('Intel Mac OS X') != -1)
    {
        directDownload = "http://go.microsoft.com/fwlink/?LinkID=87384";
    }
    //point to correct image/landing page for Alpha (0.95.x) and Beta (0.90.x)
    var inDirectDownloadPage, inDirectDownloadImage;
    
    var curVer = slProperties.version.split(".");
        majorVer = curVer[0]; 
        minorVer = curVer[1];
    
    //if Alpha, disallow inPlaceInstall
    if (minorVer == "95")
    {
        slPluginHelper.inplaceInstallPrompt = false;
        inDirectDownloadPage = "http://go.microsoft.com/fwlink/?LinkID=88363";
        inDirectDownloadImage = "http://go.microsoft.com/fwlink/?LinkID=88365";
    }
    else
    {
        inDirectDownloadPage = "http://go.microsoft.com/fwlink/?LinkID=86009";
        inDirectDownloadImage = "http://go.microsoft.com/fwlink/?LinkID=87023";    
    
    }
  
    // text for Silverlight image link, used for non-inplaceInstallPrompt and unsupported browser
    
    var silverlightLink = '<div style="width: 205px; height: 67px; background-color: #FFFFFF"><a href="'+inDirectDownloadPage+'"><img style="border:0";  src="'+inDirectDownloadImage+'"/></a></div>'
    // detect supported browser version & that the correct version of WPF/e is installed, else display install
    
    if (browserIsSupportedVersion(slPluginHelper))
    {   
      
        if (Sys.Silverlight.isInstalled(slProperties.version))
        {
            slPluginHTML = buildHTML(slPluginHelper);
        }
        else if (!slPluginHelper.inplaceInstallPrompt)
        {
            slPluginHTML = silverlightLink;
                        
        }
        else  //inPlaceInstallPrompt
        { 
            slPluginHTML += '<div style="width: 205px; height: 101px background-color: #FFFFFF;"><a href="'+directDownload+'"><img style="border:0"; SRC="http://go.microsoft.com/fwlink/?LinkID=87024"></a>';                
            slPluginHTML += '<div style="margin-top: -60px;text-align: center;color: #FFFFFF; font-size: 10px;font-family: Arial ">By clicking <b>Get Microsoft Silverlight</b> you accept the ';
            slPluginHTML += '<a href="http://go.microsoft.com/fwlink/?LinkID=87025" style="text-decoration: underline;color: #FFFFFF;">Silverlight license agreement.</a></div>';                
            slPluginHTML += '<div style="margin-top: 8px;text-align: center;color: #FFFFFF; font-family: Arial; font-size: 10px;">Silverlight updates automatically, <a href="http://go.microsoft.com/fwlink/?LinkID=87026" style="text-decoration: underline;color: #FFFFFF;">learn more.</a></div></div>';
                    
        } 
    }
    else
    {
        slPluginHTML = silverlightLink;
    
    }        
    // insert the HTML into the requested host element or return <object> tag.
    if(parentElement != null)
    {
        parentElement.innerHTML = slPluginHTML;
    }
    else 
    {
        return slPluginHTML; 
    }
        
}

///////////////////////////////////////////////////////////////////////////////
//  detect to see if this is a supported browser version    
///////////////////////////////////////////////////////////////////////////////
function browserIsSupportedVersion(slPluginHelper)
{
    var supportedBrowser = true;
    
    if (slPluginHelper.ignoreBrowserVer == true)
    {
        return supportedBrowser;
    }
    else
    {    
        var supportedBrowser = false;          
    }     
    
    // detection for Internet Explorer 6.0+, 32-bit only
    if (navigator.userAgent.indexOf('MSIE') != -1)
    {
        if (navigator.userAgent.indexOf('Win64') == -1)
        {           
            var tempVersion = navigator.userAgent.split("MSIE");
            browserMajorVersion = parseInt(tempVersion[1]);
                if (browserMajorVersion >= 6.0)
                {
                    supportedBrowser = true;
                }
        }
    }
    // detection for Firefox 1.5+ and 2.0
    else if (navigator.userAgent.indexOf("Firefox") != -1)
    {
        var tempVersion = navigator.userAgent.split("Firefox/");
        tempVersion = tempVersion[1].split(".");
        browserMajorVersion = parseFloat(tempVersion[0]);
        browserMinorVersion = parseFloat(tempVersion[1]);

        if (browserMajorVersion >= 2)
        {
            supportedBrowser = true;
        }
        else
        {
            if ((browserMinorVersion >= 5))
            {
                supportedBrowser = true;
            }
        }
    }
    else if (navigator.userAgent.indexOf("Safari") != -1)
    {
        supportedBrowser = true;
    }

    return supportedBrowser;
}

///////////////////////////////////////////////////////////////////////////////
//
//  create HTML that instantiates the control
//
///////////////////////////////////////////////////////////////////////////////
function buildHTML(slPluginHelper)
{
    var slPluginHTML = '<object type="application/ag-plugin" id="'+slPluginHelper.id+'" width="'+slPluginHelper.width+'" height="'+slPluginHelper.height+'" >';


    if (slPluginHelper.source != null)
    {
        slPluginHTML += ' <param name="source" value="'+slPluginHelper.source+'" />';
    }
    if (slPluginHelper.framerate != null)
    {
        slPluginHTML += ' <param name="maxFramerate" value="'+slPluginHelper.framerate+'" />';
    }
            
    slPluginHTML += ' <param name="onError" value="'+slPluginHelper.onError+'" />';       
    
    if (slPluginHelper.background != null)
    {
        slPluginHTML += ' <param name="background" value="'+slPluginHelper.background+'" />';
    }
    if (slPluginHelper.isWindowless != null)
    {
        slPluginHTML += ' <param name="windowless" value="'+slPluginHelper.isWindowless+'" />';        
    }
    if (slPluginHelper.initParams != null)
    {
        slPluginHTML += ' <param name="initParams" value="'+slPluginHelper.initParams+'" />';        
    }
    if (slPluginHelper.enableHtmlAccess != null)
    {
        slPluginHTML += ' <param name="enableHtmlAccess" value="'+slPluginHelper.enableHtmlAccess+'" />';        
    }
    if (slPluginHelper.loadedHandlerName != null)
    {
        slPluginHTML += ' <param name="onLoad" value="'+slPluginHelper.loadedHandlerName+'" />';        
    }
        
    slPluginHTML += '<\/object>';
    
    if (navigator.userAgent.indexOf("Safari") != -1)
   {
        // disable Safari caching
        // for more information, see http://developer.apple.com/internet/safari/faq.html#anchor5
        slPluginHTML += "<iframe style='visibility:hidden;height:0;width:0'/>";
   }

    return slPluginHTML;
}

///////////////////////////////////////////////////////////////////////////////
//
//  Default error handling function to be used when a custom error handler is
//  not present
//
///////////////////////////////////////////////////////////////////////////////

function default_error_handler(sender, args) 
{
    var iErrorCode;
    var errorType = args.ErrorType;

    iErrorCode = args.ErrorCode;

    var errMsg = "\nSilverlight error message     \n" ;

    errMsg += "ErrorCode: "+ iErrorCode + "\n";


    errMsg += "ErrorType: " + errorType + "       \n";
    errMsg += "Message: " + args.ErrorMessage + "     \n";

    if (errorType == "ParserError")
    {
        errMsg += "XamlFile: " + args.xamlFile + "     \n";
        errMsg += "Line: " + args.lineNumber + "     \n";
        errMsg += "Position: " + args.charPosition + "     \n";
    }
    else if (errorType == "RuntimeError")
    {           
        if (args.lineNumber != 0)
        {
            errMsg += "Line: " + args.lineNumber + "     \n";
            errMsg += "Position: " +  args.charPosition + "     \n";
        }
        errMsg += "MethodName: " + args.methodName + "     \n";
    }

    alert(errMsg);
}


// createObjectEx, takes a single parameter of all createObject parameters enclosed in {}
        
Sys.Silverlight.createObjectEx = function(params)
{        
    var parameters = params;
    Sys.Silverlight.createObject(parameters.source, parameters.parentElement, parameters.id, parameters.properties, parameters.events, parameters.initParams, parameters.context)
}
 