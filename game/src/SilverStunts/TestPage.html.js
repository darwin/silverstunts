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
	Sys.Silverlight.createObjectEx({
		source: xaml,
		parentElement: document.getElementById(host),
		id: id,
		properties: {
			width: "100%",
			height: "100%",
			version: "0.95",
			enableHtmlAccess: true
		},
		events: {
		  onLoad: initSilverLightControl
		}
	});
}
