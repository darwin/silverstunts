using System;
using System.Windows.Controls;
using System.Xml;
using System.Diagnostics;
using System.Text;
using System.Collections.Generic;
using System.Reflection;

using Physics.Base;
using Physics.Surfaces;

namespace SilverStunts
{
	public class Visual : Control
	{
		public enum Family
		{
			Line,
			Rectangle,
			Circle,
			Bike,
			Gizmo,
			Other
		}

		public enum RenderMode
		{
			Init,
			Output
		}

		public PhysicsObject source;
		public Canvas content;
		public Family family;
		public string selector;
		BindingTable bindings;

		class Binding
		{
			int id;
			string attribute;
			string field;

			public Binding(int id, string attribute, string field)
			{
				this.id = id;
				this.attribute = attribute;
				this.field = field;
			}

			public object GetValue(Object source)
			{
				BindingFlags f = BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
				string[] names = field.Split('.');
				Object value;
				try
				{
					value = source;
					for (int i = 0; i < names.Length; i++)
					{
						Type st = value.GetType();
						value = st.InvokeMember("get_" + names[i], f | BindingFlags.InvokeMethod, null, value, new object[] { });
					}
				}
				catch (System.MissingMethodException)
				{
					Debug.WriteLine("Missing source field {0} on {1}", field, source.ToString());
					return null;
				}

				return value;
			}

			public void SetValue(Object target, Object value)
			{
				BindingFlags f = BindingFlags.IgnoreCase | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
				try
				{
					Type tt = target.GetType();
					tt.InvokeMember("set_" + attribute, f | BindingFlags.InvokeMethod, null, target, new object[] { value });
				}
				catch (System.MissingMethodException)
				{
					Debug.WriteLine("Missing target attribute {0} on {1}", attribute, target.ToString());
					return;
				}
			}

			public void Update(Canvas content, Object source)
			{
				// HACK: update opacity for gizmos
				if (source is AbstractSurface)
				{
					AbstractSurface tile = (AbstractSurface)source;
					content.Opacity = tile.Active ? 1.0 : 0.3;
				}

				// generic updating system
				Object target = content.FindName(id.ToString());
				Object value = GetValue(source);
				SetValue(target, value);
			}
		}

		class BindingTable : List<Binding>
		{
			int counter = 0;

			public void NewElement()
			{
				counter++;
			}

			public int CurrentId
			{
				get { return counter; }
			}
		}

		public Visual(PhysicsObject o, Family type, string selector)
		{
			this.source = o;
			this.family = type;
			this.selector = selector;

			System.IO.Stream stream = this.GetType().Assembly.GetManifestResourceStream("SilverStunts.Visuals.xml");
			XmlReader reader = XmlReader.Create(stream);

			reader.ReadToFollowing("Visual");
			do
			{
				string c = source.GetType().FullName;
				int lastDot = c.LastIndexOf('.');
				c = c.Substring(lastDot+1);

				if (reader.GetAttribute("Type") == c)
				{
					Initialize(reader.ReadSubtree());
					break;
				}
			}
			while (reader.ReadToNextSibling("Visual"));
		}

		void ProcessNode(XmlReader reader, XmlWriter writer, BindingTable bindingTable, RenderMode mode)
		{
			if (reader == null) throw new ArgumentNullException("reader");
			if (writer == null) throw new ArgumentNullException("writer");

			switch (reader.NodeType)
			{
				case XmlNodeType.Element:
					bindingTable.NewElement();
					writer.WriteStartElement(reader.Prefix, reader.LocalName, reader.NamespaceURI);

					if (reader.HasAttributes)
					{
						if (mode == RenderMode.Init)
						{
							string nodeName = bindingTable.CurrentId.ToString();
							writer.WriteStartAttribute("Name");
							writer.WriteString(nodeName);
							writer.WriteEndAttribute();
						}
						for (int i = 0; i < reader.AttributeCount; i++)
						{
							reader.MoveToAttribute(i);

							string name = reader.Name;
							string value = reader.Value.Trim();

							if (value.StartsWith("{"))
							{
								string field = value.Substring(1, value.Length - 2);
								Binding d = new Binding(bindingTable.CurrentId, name, field);
								Object val = d.GetValue(source);
								if (val is Double)
								{
									double num = (double)val;
									// don't mess the value with locale settings
									value = num.ToString(System.Globalization.NumberFormatInfo.InvariantInfo);
								}
								else
								{
									value = val.ToString();
								}
								if (mode == RenderMode.Init)
								{
									bindingTable.Add(d);
								}
							}
							writer.WriteAttributeString(name, value);
						}
						reader.MoveToContent();
					}
					if (reader.IsEmptyElement)
					{
						writer.WriteEndElement();
					}
					break;

				case XmlNodeType.Text:
					writer.WriteString(reader.Value);
					break;

				case XmlNodeType.Whitespace:
				case XmlNodeType.SignificantWhitespace:
					writer.WriteWhitespace(reader.Value);
					break;

				case XmlNodeType.CDATA:
					writer.WriteCData(reader.Value);
					break;

				case XmlNodeType.EntityReference:
					writer.WriteEntityRef(reader.Name);
					break;

				case XmlNodeType.XmlDeclaration:
				case XmlNodeType.ProcessingInstruction:
					writer.WriteProcessingInstruction(reader.Name, reader.Value);
					break;

				case XmlNodeType.DocumentType:
					writer.WriteDocType(reader.Name, reader.GetAttribute("PUBLIC"), reader.GetAttribute("SYSTEM"), reader.Value);
					break;

				case XmlNodeType.Comment:
					writer.WriteComment(reader.Value);
					break;

				case XmlNodeType.EndElement:
					writer.WriteFullEndElement();
					break;
			}
		}

		public string RenderXAML()
		{
			System.IO.Stream stream = this.GetType().Assembly.GetManifestResourceStream("SilverStunts.Visuals.xml");
			XmlReader reader = XmlReader.Create(stream);

			reader.ReadToFollowing("Visual");
			do
			{
				string c = source.GetType().FullName;
				int lastDot = c.LastIndexOf('.');
				c = c.Substring(lastDot + 1);

				if (reader.GetAttribute("Type") == c) return RenderXAML(reader.ReadSubtree(), RenderMode.Output);
			}
			while (reader.ReadToNextSibling("Visual"));
			return "";
		}

		string RenderXAML(XmlReader reader, RenderMode mode)
		{
			XmlWriterSettings settings = new XmlWriterSettings();
			settings.Indent = true;
			settings.CheckCharacters = true;
			settings.CloseOutput = false;
			settings.OmitXmlDeclaration = true;
			settings.ConformanceLevel = ConformanceLevel.Fragment;

			StringBuilder output = new StringBuilder();
			XmlWriter writer = XmlWriter.Create(output, settings);
			while (reader.Read())
			{
				writer.Flush(); // HACK
				if (reader.Depth > 0) ProcessNode(reader, writer, bindings, mode);
			}
			writer.Flush();
			writer.Close();

			string name = "x:Name=\"" + source.name + "\"";
			if (source.name == "?") name = "";

			string dimensions = "";
			if (content != null)
			{
				dimensions = String.Format("Width=\"{0}\" Height=\"{1}\"", content.Width, content.Height);
			}

			return "<Canvas " + name + " "+ dimensions +">" + output.ToString() + "</Canvas>";
		}

		private void Initialize(XmlReader reader)
		{
			bindings = new BindingTable();
			string markup = RenderXAML(reader, RenderMode.Init);
			content = (Canvas)this.InitializeFromXaml(markup);
		}

		public void Update()
		{
			// process bindings in the table
			foreach (Binding binding in bindings)
			{
				binding.Update(content, source);
			}
		}
	}
}
