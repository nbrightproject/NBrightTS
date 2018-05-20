// use {id} to replace the control id in the script.

 var editorvar{id} = '';

 $(document).ready(function () {

 CKEDITOR.plugins.addExternal( 'simplebox', '/DesktopModules/NBright/NBrightData/ckeditor/plugins/simplebox/', 'plugin.js' );

 editorvar{id} = CKEDITOR.replace('editor{id}', { extraPlugins: 'simplebox', customConfig: '/DesktopModules/NBright/NBrightData/ckeditor/nbrightconfig.js' } );

 editorvar{id}.on('change', function (event) {  
	var value = editorvar{id}.getData();
	$('#{id}').val(value); }); }
);
 