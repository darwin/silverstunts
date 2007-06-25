var EditArea_console= {
	/**
	 * Get called once this file is loaded (editArea still not initialized)
	 *
	 * @return nothing	 
	 */	 	 	
	init: function(){	
	}
	
	/**
	 * Returns the HTML code for a specific control string or false if this plugin doesn't have that control.
	 * A control can be a button, select list or any other HTML item to present in the EditArea user interface.
	 * Language variables such as {$lang_somekey} will also be replaced with contents from
	 * the language packs.
	 * 
	 * @param {string} ctrl_name: the name of the control to add	  
	 * @return HTML code for a specific control or false.
	 * @type string	or boolean
	 */	
	,get_control_html: function(ctrl_name){
		return false;
	}
	/**
	 * Get called once EditArea is fully loaded and initialised
	 *	 
	 * @return nothing
	 */	 	 	
	,onload: function(){ 
	}
	
	/**
	 * Is called each time the user touch a keyboard key.
	 *	 
	 * @param (event) e: the keydown event
	 * @return true - pass to next handler in chain, false - stop chain execution
	 * @type boolean	 
	 */
	,onkeydown: function(e){
	    if (e.keyCode==13)
	    {
	        if (editArea.nav['isIE'])
			    editArea.getIESelection();
			    
		    var start = editArea.textarea.selectionStart;
		    var end = editArea.textarea.selectionEnd;
		    var start_last_line= Math.max(0 , editArea.textarea.value.substring(0, start).lastIndexOf("\n") + 1 );
		    var begin_line= editArea.textarea.value.substring(start_last_line, start).replace(/^([ \t]*).*/gm, "$1");
		    //alert(start_last_line+" start: "+start +"\n"+editArea.textarea.value.substring(start_last_line, start));
		    
		    var code = editArea.textarea.value.substring(start_last_line, start);
		    code = code.replace(/\'/g, "\\'");
		    var ctrl = "false"; if (e.ctrlKey) ctrl = "true";
            if (editArea.settings["console_callback"].length>0)
					eval("parent."+editArea.settings["console_callback"]+"('"+ code +"', "+ctrl+");");		    
	    }
		return true;
	}
	
	/**
	 * Executes a specific command, this function handles plugin commands.
	 *
	 * @param {string} cmd: the name of the command being executed
	 * @param {unknown} param: the parameter of the command	 
	 * @return true - pass to next handler in chain, false - stop chain execution
	 * @type boolean	
	 */
	,execCommand: function(cmd, param){
		// Pass to next handler in chain
		return true;
	}
	
};

// Adds the plugin class to the list of available EditArea plugins
editArea.add_plugin("console", EditArea_console);
