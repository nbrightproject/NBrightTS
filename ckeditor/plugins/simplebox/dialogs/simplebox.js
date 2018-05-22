/**
 * Copyright (c) 2014-2018, CKSource - Frederico Knabben. All rights reserved.
 * Licensed under the terms of the MIT License (see LICENSE.md).
 */

// Note: This automatic widget to dialog window binding (the fact that every field is set up from the widget
// and is committed to the widget) is only possible when the dialog is opened by the Widgets System
// (i.e. the widgetDef.dialog property is set).
// When you are opening the dialog window by yourself, you need to take care of this by yourself too.

CKEDITOR.dialog.add( 'simplebox', function( editor ) {
	return {
		title: 'Edit Simple Box',
		minWidth: 200,
		minHeight: 100,
		contents: [
			{
				id: 'info',
				elements: [
					{
						id: 'align',
						type: 'select',
						label: 'Align',
						items: [
							[ editor.lang.common.notSet, '' ],
							[ editor.lang.common.alignLeft, 'left' ],
							[ editor.lang.common.alignRight, 'right' ],
							[ editor.lang.common.alignCenter, 'center' ]
						],
						// When setting up this field, set its value to the "align" value from widget data.
						// Note: Align values used in the widget need to be the same as those defined in the "items" array above.
						setup: function( widget ) {
							this.setValue( widget.data.align );
						},
						// When committing (saving) this field, set its value to the widget data.
						commit: function( widget ) {
							widget.setData( 'align', this.getValue() );
						}
					},
					{
						id: 'width',
						type: 'text',
						label: 'Width',
						width: '50px',
						setup: function( widget ) {
							this.setValue( widget.data.width );
						},
						commit: function( widget ) {
							widget.setData( 'width', this.getValue() );
						}
					},
					{
                        id: 'width2',
                        type: 'text',
                        label: 'Width2',
                        width: '150px',
                        setup: function (widget) {
                            this.setValue(widget.data.width);
                        },
                        commit: function (widget) {
                            widget.setData('width', this.getValue());
                        }
                    },
                    {
                        type: 'select',
                        id: 'dnnpage',
                        width: '150px',
                        label: 'Page',
                        items: [],
                        setup: function (element) {
                            var element_id = '#' + this.getInputElement().$.id;
                            $.ajax({
                                type: 'POST',
                                url: '/DesktopModules/NBright/NBrightData/ApiConnector.ashx?cmd=dnnpages',
                                data: '{"cpID":' + window.parent.$("#cpID").val() + '}',
                                contentType: 'text/plain; charset=utf-8',
                                dataType: 'text',
                                async: false,
                                success: function (data) {
                                    if (window.DOMParser) {
                                        parser = new DOMParser();
                                        xmlDoc = parser.parseFromString(data, "text/xml");
                                    }
                                    else // Internet Explorer
                                    {
                                        xmlDoc = new ActiveXObject("Microsoft.XMLDOM");
                                        xmlDoc.async = false;
                                        xmlDoc.loadXML(data);
                                    }

                                    var list = xmlDoc.getElementsByTagName("page");
                                    for (var i = 0; i < list.length; i++) {
                                        //alert(list[i].getAttribute("url"));
                                        $(element_id).get(0).options[$(element_id).get(0).options.length] = new Option(list[i].firstChild.nodeValue, list[i].getAttribute("url"));
                                    }

                                },
                                error: function (xhr, ajaxOptions, thrownError) {
                                    alert(xhr.status);
                                    alert(thrownError);
                                }
                            });
                        }

                    }

				]
			}
		]
	};
} );