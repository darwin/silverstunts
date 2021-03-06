/******
 *
 *	EditArea 
 * 	Developped by Christophe Dolivet
 *	Released under LGPL license
 *
******/

	function EditArea(){
		this.error= false;	// to know if load is interrrupt
		
		this.inlinePopup= new Array({popup_id: "area_search_replace", icon_id: "search"},
									{popup_id: "edit_area_help", icon_id: "help"});
		this.plugins= new Object();
	
		this.line_number=0;
		
		this.nav=parent.editAreaLoader.nav; 	// navigator identification
		
		this.last_selection=new Object();		
		this.last_text_to_highlight="";
		this.last_hightlighted_text= "";
		
		this.textareaFocused= false;
		this.previous= new Array();
		this.next= new Array();
		this.last_undo="";
		//this.loaded= false;
		this.assocBracket=new Object();
		this.revertAssocBracket= new Object();		
		// bracket selection init 
		this.assocBracket["("]=")";
		this.assocBracket["{"]="}";
		this.assocBracket["["]="]";		
		for(var index in this.assocBracket){
			this.revertAssocBracket[this.assocBracket[index]]=index;
		}
		/*this.textarea="";	
		
		this.state="declare";
		this.code = new Array(); // store highlight syntax for languagues*/
		// font datas
		this.lineHeight= 16;
		/*this.default_font_family= "monospace";
		this.default_font_size= 10;*/
		this.tab_nb_char= 8;	//nb of white spaces corresponding to a tabulation
		if(this.nav['isOpera'])
			this.tab_nb_char= 6;

		this.is_tabbing= false;
		
		this.fullscreen= {'isFull': false};
		
		this.isResizing=false;	// resize var
		
		// init with settings and ID
		this.id= area_id;
		this.settings= editAreas[this.id]["settings"];
		
		if((""+this.settings['replace_tab_by_spaces']).match(/^[0-9]+$/))
		{
			this.tab_nb_char= this.settings['replace_tab_by_spaces'];
			this.tabulation="";
			for(var i=0; i<this.tab_nb_char; i++)
				this.tabulation+=" ";
		}else
			this.tabulation="\t";
	};
	
	
	//called by the toggle_on
	EditArea.prototype.update_size= function(){
		
		if(editAreas[editArea.id] && editAreas[editArea.id]["displayed"]==true){
			if(editArea.fullscreen['isFull']){	
                                parent.document.getElementById("frame_"+editArea.id).style.top="21px";
				parent.document.getElementById("frame_"+editArea.id).style.width= parent.document.getElementsByTagName("html")[0].clientWidth + "px";
				parent.document.getElementById("frame_"+editArea.id).style.height= (parent.document.getElementsByTagName("html")[0].clientHeight - 20) + "px";
			}
			var height= document.body.offsetHeight - editArea.get_all_toolbar_height() - 4;
			if (height<0) height = 0;
			editArea.result.style.height= height +"px";
			
			var width=document.body.offsetWidth -2;
			if (width<0) width = 0;
			editArea.result.style.width= width+"px";
			//alert("result h: "+ height+" w: "+width+"\ntoolbar h: "+this.get_all_toolbar_height()+"\nbody_h: "+document.body.offsetHeight);
			
			// check that the popups don't get out of the screen
			for(var i=0; i<editArea.inlinePopup.length; i++){
				var popup= document.getElementById(editArea.inlinePopup[i]["popup_id"]);
				var max_left= document.body.offsetWidth- popup.offsetWidth;
				var max_top= document.body.offsetHeight- popup.offsetHeight;
				if(popup.offsetTop>max_top)
					popup.style.top= max_top+"px";
				if(popup.offsetLeft>max_left)
					popup.style.left= max_left+"px";
			}
		}		
	};

	EditArea.prototype.init= function(){
		this.textarea= document.getElementById("textarea");
		this.container= document.getElementById("container");
		this.result= document.getElementById("result");
		this.content_highlight= document.getElementById("content_highlight");
		this.selection_field= document.getElementById("selection_field");
		
		
		// add plugins buttons in the toolbar
		spans= parent.getChildren(document.getElementById("toolbar_1"), "span", "", "", "all", -1);
		
		for(var i=0; i<spans.length; i++){
		
			id=spans[i].id.replace(/tmp_tool_(.*)/, "$1");
			if(id!= spans[i].id){
				for(var j in this.plugins){
					if(typeof(this.plugins[j].get_control_html)=="function" ){
						html=this.plugins[j].get_control_html(id);
						if(html!=false){
							html= parent.editAreaLoader.translate(html, this.settings["language"], "template");
							var new_span= document.createElement("span");
							new_span.innerHTML= html;				
							var father= spans[i].parentNode;
							spans[i].parentNode.replaceChild(new_span, spans[i]);	
							break; // exit the for loop					
						}
					}
				}
			}
		}
		
		
		
		// init datas
		this.textarea.value=editAreas[this.id]["textarea"].value;
		if(this.settings["debug"])
			this.debug=parent.document.getElementById("edit_area_debug_"+this.id);
		
		// init size		
		//this.update_size();
		
		if(document.getElementById("redo") != null)
			this.switchClassSticky(document.getElementById("redo"), 'editAreaButtonDisabled', true);
		
		
		// insert css rules for highlight mode		
		if(typeof(parent.editAreaLoader.syntax[this.settings["syntax"]])!="undefined"){
			for(var i in parent.editAreaLoader.syntax){
				this.add_style(parent.editAreaLoader.syntax[i]["styles"]);
			}
		}
		// init key events
		if(this.nav['isOpera'])
			document.getElementById("editor").onkeypress= keyDown;
		else
			document.getElementById("editor").onkeydown= keyDown;
	/*	if(this.nav['isIE'] || this.nav['isFirefox'])
			this.textarea.onkeydown= keyDown;
		else if
			this.textarea.onkeypress= keyDown;*/
		for(var i=0; i<this.inlinePopup.length; i++){
			if(this.nav['isIE'] || this.nav['isFirefox'])
				document.getElementById(this.inlinePopup[i]["popup_id"]).onkeydown= keyDown;
			else
				document.getElementById(this.inlinePopup[i]["popup_id"]).onkeypress= keyDown;
		}
		
		if(this.settings["allow_resize"]=="both" || this.settings["allow_resize"]=="x" || this.settings["allow_resize"]=="y")
			this.allow_resize(true);
		
		parent.editAreaLoader.toggle(this.id, "on");
		//this.textarea.focus();
		// line selection init
		this.change_smooth_selection_mode(editArea.smooth_selection);
		// highlight
		this.execCommand("change_highlight", this.settings["start_highlight"]);
		
		// get font size datas		
		this.set_font(editArea.settings["font_family"], editArea.settings["font_size"]);
		
		// set unselectable text
		children= parent.getChildren(document.body, "", "selec", "none", "all", -1);
		for(var i=0; i<children.length; i++){
			if(this.nav['isIE'])
				children[i].unselectable = true; // IE
			else
				children[i].onmousedown= function(){return false};
		/*	children[i].style.MozUserSelect = "none"; // Moz
			children[i].style.KhtmlUserSelect = "none";  // Konqueror/Safari*/
		}
		
		if(this.nav['isGecko']){
			this.textarea.spellcheck= this.settings["gecko_spellcheck"];
		}
		
		if(this.nav['isOpera']){
			document.getElementById("editor").style.position= "absolute";
			document.getElementById("selection_field").style.marginTop= "-1pt";			
			document.getElementById("selection_field").style.paddingTop= "1pt";
			document.getElementById("cursor_pos").style.marginTop= "-1pt";
			document.getElementById("end_bracket").style.marginTop= "-1pt";
			document.getElementById("content_highlight").style.marginTop= "-1pt";
			/*document.getElementById("end_bracket").style.marginTop="1px";*/
		}
		
		// si le textarea n'est pas grand, un click sous le textarea doit provoquer un focus sur le textarea
		parent.editAreaLoader.add_event(this.result, "click", function(e){ if((e.target || e.srcElement)==editArea.result) { editArea.area_select(editArea.textarea.value.length, 0);}  });
		
		setTimeout("editArea.manage_size();editArea.execCommand('EA_load');", 10);		
		//start checkup routine
		this.check_undo();
		this.check_line_selection(true);
		this.scroll_to_view();
		
		
		for(var i in this.plugins){
			if(typeof(this.plugins[i].onload)=="function")
				this.plugins[i].onload();
		}
		if(this.settings['fullscreen']==true)
			this.toggle_full_screen(true);
		
		parent.editAreaLoader.add_event(window, "resize", editArea.update_size);
		parent.editAreaLoader.add_event(parent.window, "resize", editArea.update_size);
		parent.editAreaLoader.add_event(top.window, "resize", editArea.update_size);
		parent.editAreaLoader.add_event(window, "unload", function(){if(editAreas[editArea.id] && editAreas[editArea.id]["displayed"]) editArea.execCommand("EA_unload");});
		
		/*date= new Date();
		alert(date.getTime()- parent.editAreaLoader.start_time);*/
	};
	
	EditArea.prototype.manage_size= function(){
		if(!editAreas[this.id])
			return false;
		if(editAreas[this.id]["displayed"]==true)
		{
			var resized= false;
			area_width= this.textarea.scrollWidth;
			area_height= this.textarea.scrollHeight;
			if(this.nav['isOpera']){
				area_height= this.last_selection['nb_line']*this.lineHeight;
				//area_width-=45;
				area_width=10000; /* TODO: find a better way to fix the width problem */
				//elem= document.getElementById("container");
				//elem= this.textarea;
				//window.status="area over: area_width "+area_width+" scroll: "+elem.scrollWidth+" offset: "+elem.offsetWidth +" client: "+ elem.clientWidth+" style: "+elem.style.width;
				//window.status+=" area_height "+area_height+" scroll: "+elem.scrollHeight+" offset: "+elem.offsetHeight +" client: "+ elem.clientHeight;				
			}
			
			if(this.nav['isIE']==7)
				area_width-=45;
	
			if(this.nav['isGecko'] && this.smooth_selection && this.last_selection["nb_line"])
				area_height= this.last_selection["nb_line"]*this.lineHeight;
				
			if(this.last_selection["nb_line"] >= this.line_number)
			{
				var div_line_number="";
				for(i=this.line_number+1; i<this.last_selection["nb_line"]+100; i++)
				{
					div_line_number+=i+"<br />";
					this.line_number++;
				}
				var span= document.createElement("span");
				if(this.nav['isIE'])
					span.unselectable=true;
				span.innerHTML=div_line_number;					
				document.getElementById("line_number").appendChild(span);				
			}
			//alert(area_height);
				
			if(this.textarea.previous_scrollWidth!=area_width)
			{	// need width resizing
				if(this.nav['isOpera']){
					/*if(this.textarea.style.width.replace("px","")-0+50 < area_width)
						area_width+=50;*/
				}else{
					if(this.textarea.style.width && (this.textarea.style.width.replace("px","") < area_width)){
						area_width+=50;
					}
				}
				//window.status= "width: "+this.textarea.offsetWidth+" scroll-width: "+this.textarea.scrollWidth+" area_width: "+area_width+" container: "+this.container.offsetWidth+" result: "+this.result.offsetWidth;
		
				if(this.nav['isGecko'] || this.nav['isOpera'])
					this.container.style.width= (area_width+45)+"px";
				else
				{
					if (area_width<0) area_width  =0;
					this.container.style.width= area_width+"px";
				}
				this.textarea.style.width= area_width+"px";
				this.content_highlight.style.width= area_width+"px";	
				this.textarea.previous_scrollWidth=area_width;
				resized=true;
			}		
			if(this.textarea.previous_scrollHeight!=area_height)	
			{	// need height resizing
				/*container_height=area_height;
				if(document.getElementById("container").style.height.replace("px", "")<=area_height)
					container_height+=100;*/
				this.container.style.height= (area_height+2)+"px";
				this.textarea.style.height= area_height+"px";
				this.content_highlight.style.height= area_height+"px";	
				this.textarea.previous_scrollHeight=area_height;
				//alert(area_height);
				resized=true;
			}
			this.textarea.scrollTop="0px";
			this.textarea.scrollLeft="0px";
			if(resized==true){
				this.scroll_to_view();
			}
		}
		setTimeout("editArea.manage_size();", 100);
	};
	
	EditArea.prototype.add_event = function(obj, name, handler) {
		if (this.nav['isIE']) {
			obj.attachEvent("on" + name, handler);
		} else{
			obj.addEventListener(name, handler, false);
		}
	};
	
	EditArea.prototype.execCommand= function(cmd, param){
		
		for(var i in this.plugins){
			if(typeof(this.plugins[i].execCommand)=="function"){
				if(!this.plugins[i].execCommand(cmd, param))
					return;
			}
		}
		switch(cmd){
			case "save":
				if(this.settings["save_callback"].length>0)
					eval("parent."+this.settings["save_callback"]+"('"+ this.id +"', editArea.textarea.value);");
				break;
			case "load":
				if(this.settings["load_callback"].length>0)
					eval("parent."+this.settings["load_callback"]+"('"+ this.id +"');");
				break;
			case "onchange":
				if(this.settings["change_callback"].length>0)
					eval("parent."+this.settings["change_callback"]+"('"+ this.id +"');");
				break;		
			case "EA_load":
				if(this.settings["EA_load_callback"].length>0)
					eval("parent."+this.settings["EA_load_callback"]+"('"+ this.id +"');");
				break;
			case "EA_unload":
				if(this.settings["EA_unload_callback"].length>0)
					eval("parent."+this.settings["EA_unload_callback"]+"('"+ this.id +"');");
				break;
			case "toggle_on":
				if(this.settings["EA_toggle_on_callback"].length>0)
					eval("parent."+this.settings["EA_toggle_on_callback"]+"('"+ this.id +"');");
				break;
			case "toggle_off":
				if(this.settings["EA_toggle_off_callback"].length>0)
					eval("parent."+this.settings["EA_toggle_off_callback"]+"('"+ this.id +"');");
				break;
			case "re_sync":
				if(!this.do_highlight)
					break;
			default:
				//alert(cmd+"\n"+params);
				if(typeof(eval("editArea."+cmd))=="function")
					try{eval("editArea."+ cmd +"(param);");}catch(e){};	
		}
	};
	
	EditArea.prototype.get_translation= function(word, mode){
		if(mode=="template")
			return parent.editAreaLoader.translate(word, this.settings["language"], mode);
		else
			return parent.editAreaLoader.get_word_translation(word, this.settings["language"]);
	};
	
	EditArea.prototype.add_plugin= function(plug_name, plug_obj){
		for(var i=0; i<this.settings["plugins"].length; i++){
			if(this.settings["plugins"][i]==plug_name){
				this.plugins[plug_name]=plug_obj;
				plug_obj.baseURL=parent.editAreaLoader.baseURL + "plugins/" + plug_name + "/";
				if( typeof(plug_obj.init)=="function" )
					plug_obj.init();
			}
		}
	};
	
	EditArea.prototype.load_css= function(url){
		try{
			link = document.createElement("link");
			link.type = "text/css";
			link.rel= "stylesheet";
			link.media="all";
			link.href = url;
			head = document.getElementsByTagName("head");
			head[0].appendChild(link);
		}catch(e){
			document.write("<link href='"+ url +"' rel='stylesheet' type='text/css' />");
		}
	};
	
	EditArea.prototype.load_script= function(url){
		try{
			script = document.createElement("script");
			script.type = "text/javascript";
			script.src  = url;
			head = document.getElementsByTagName("head");
			head[0].appendChild(script);
		}catch(e){
			document.write("<script type='text/javascript' src='" + url + "'><"+"/script>");
		}
	};
	
	// add plugin translation to language translation array
	EditArea.prototype.add_lang= function(language, values){
		if(!parent.editAreaLoader.lang[language])
			parent.editAreaLoader.lang[language]=new Object();
		for(var i in values)
			parent.editAreaLoader.lang[language][i]= values[i];
	};
	

	var editArea = new EditArea();	
	editArea.add_event(window, "load", init);
	
	function init(){		
		setTimeout("editArea.init();  ", 10);
	};
	
