// JScript source code
function initSilverLightControl(sender, args)
{
    sender.Content.page.PrintConsoleEvent = OnPrintConsole;
    sender.Content.page.ShowWorldXAMLEvent = OnShowWorldXAML;
    sender.Content.page.RequestEntitiesSourceEvent = OnRequestEntitiesSource;
    sender.Content.page.PublishEntitiesSourceEvent = OnPublishEntitiesSource;
    sender.Content.page.OpenScriptEditorEvent = OnOpenScriptEditor;
    sender.Content.page.UpdateIDEEvent = OnUpdateIDE;
}

//contains calls to silverlight.js, example below loads Page.xaml
function createSilverlight(xaml, host, id)
{
    Silverlight.createObject(
        xaml, 
        document.getElementById(host), 
        id,
        {
            width:'100%', 
            height:'100%',
            version:'1.1'
            //enableHtmlAccess: true
        },
        { 
            onLoad: initSilverLightControl
        },
        null
   );
}
