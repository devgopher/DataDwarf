/*
 * Пользователь: Igor
 * Дата: 28.02.2016
 * Время: 22:23
 */
using System;
using System.Xml;
using System.Collections.Generic;

namespace DwarfServer
{
	public class ClientMessage : Message {
		public Dictionary<string, string> Contents =
			new Dictionary<string, string>();
		
		/// <summary>
		/// Parses an XML document into a "Message" object
		/// </summary>
		/// <param name="in_contents"></param>
		public override void FromXML( string in_contents ) {
			try {
				XmlDocument doc = new XmlDocument();
				doc.LoadXml( in_contents );				
				var root_element = doc.DocumentElement;				
				Contents.Clear();				
				
				foreach( XmlNode node in root_element.ChildNodes ) {
					if ( node.Name == "type") {
						Type = node.InnerText;
					}
					
					if ( node.Name == "field" ) {
						// seeking for attributes
						var node_attrs = node.Attributes;
						if (  node_attrs != null ) {
							// filling Contents dictionary
							if (node_attrs["field_name"] != null && node.InnerText != null )
								Contents[node_attrs["field_name"].Value] = node.InnerText;
						}
					}
				}
			} catch ( XmlException xe ) {
				Logger.Logger
					.GetInstance()
					.WriteError("Error in XML parsing: "+xe.Message+": line "+xe.LinePosition);
			}
		}
		
		/// <summary>
		/// Converts a message into XML document
		/// </summary>
		/// <returns></returns>
		public override string ToXml() {
			XmlDocument new_xml = new XmlDocument();
			var root_elem = new_xml.CreateElement("root");
			string ret = String.Empty;
			
			XmlElement type = new_xml.CreateElement("type");
			type.InnerText = Type;
			root_elem.AppendChild( type );
			
			foreach ( var key in Contents.Keys ) {
				XmlElement chld = new_xml.CreateElement("field");
				
				var fld_name_attr = new_xml.CreateAttribute( "field_name" );
				fld_name_attr.Value = key;
				
				chld.Attributes.Append( fld_name_attr );
				chld.InnerText = Contents[key];
				root_elem.AppendChild( chld );
			}
			
			ret = root_elem.OuterXml;
			
			return ret;
		}
	}
}
