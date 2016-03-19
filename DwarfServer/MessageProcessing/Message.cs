/*
 * Пользователь: Igor
 * Дата: 28.02.2016
 * Время: 22:22
 */
using System;
using System.Xml;

namespace DwarfServer
{
	public abstract class Message {
		protected Message() {
			Timestamp = DateTime.UtcNow;
		}
		
		public DateTime Timestamp {
			get; private set;
		}
		public string Type {
			get; set;
		}
		
		protected void TimeFromXml( XmlElement root_elem ) {
			try {
				var attr = root_elem.Attributes["Timestamp"];
				Timestamp = DateTime.Parse( attr.Value );
			} catch ( Exception ex ) {
				Logger.Logger.GetInstance().WriteError("Error occured " +
				                                       "while parsing DateTime: "+ex.Message);
			}
		}
		
		protected void TimeStampToXml( XmlElement root_element ) {
			try {
				root_element.SetAttribute("Timestamp", String.Empty,
				                          Timestamp.ToUniversalTime().ToString());
			} catch ( Exception ex ) {
				Logger.Logger.GetInstance().WriteError("Error occured " +
				                                       "while setting DateTime: "+ex.Message);
			}
		}
		
		public abstract void FromXML( string in_contents );
		
		public abstract string ToXml();
	}
}
