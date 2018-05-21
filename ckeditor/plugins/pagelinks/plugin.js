/**
 * Copyright (c) 2014-2018, CKSource - Frederico Knabben. All rights reserved.
 * Licensed under the terms of the MIT License (see LICENSE.md).
 *
 * Basic sample plugin inserting current date and time into the CKEditor editing area.
 *
 * Created out of the CKEditor Plugin SDK:
 * http://docs.ckeditor.com/ckeditor4/docs/#!/guide/plugin_sdk_intro
 */

// Register the plugin within the editor.
CKEDITOR.plugins.add( 'pagelinks', {

	editor.addCommand( 'abbr', new CKEDITOR.dialogCommand( 'abbrDialog' ) );

	// Register the icons. They must match command names.
	icons: 'pagelinks',

	// The plugin initialization logic goes inside this method.
	init: function( editor ) {

		// Define the editor command that inserts a timestamp.
		editor.addCommand( 'insertPagelinks', {

			// Define the function that will be fired when the command is executed.
			exec: function( editor ) {
				var now = new Date();

				// Insert the timestamp into the document.
				editor.insertHtml( 'The current date and time is: <em>' + now.toString() + '</em>' );
			}
		});

	
		// Create the toolbar button that executes the above command.
		editor.ui.addButton( 'Pagelinks', {
			label: 'Insert Pagelinks',
			command: 'insertPagelinks',
			toolbar: 'insert,100'
		});
	}
});
