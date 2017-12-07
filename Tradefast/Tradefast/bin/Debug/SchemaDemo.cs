using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.IO;


namespace SchemaDemo {


   /// <summary>
   /// Program to demonstrate getting XML schema information during document validation
   /// </summary>
   class Program {


      /// <summary>
      /// Standard program entry point
      /// </summary>
      /// <param name="args">command line argumemnts (not used)</param>
      static void Main(string[] args) {
         string innerText = null;
         XmlSchema schema = new XmlSchema();


         //   get the schema and instance document from the project directory
         FileStream schemaStream = new FileStream(@"..\..\sample.xsd", FileMode.Open, FileAccess.Read);
         FileStream docStream = new FileStream(@"..\..\sample.xml", FileMode.Open, FileAccess.Read);


         try {


            //   create a validating reader
            schema = XmlSchema.Read(schemaStream,
               delegate(Object sender, ValidationEventArgs e) {throw new Exception("document validation failed: " + e.Message);});
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.ValidationType = ValidationType.Schema;
            settings.Schemas.Add(schema);
            settings.ValidationEventHandler +=
               delegate(Object sender, ValidationEventArgs e) {throw new Exception("document validation failed: " + e.Message);};
            XmlReader reader = XmlReader.Create(docStream, settings);


            //   loop through the documnent
            while (reader.Read()) {


               //   dump information about any attributes
               if (reader.HasAttributes) {
                  while (reader.MoveToNextAttribute()) {
                     string data = reader.Value;
                     describe(reader, data);
                  }
               }


               //   Capture the an elements text.  This is necessary because
               //   the schema information isn't available when the reader is
               //   on the text node; it is available only when the reader is
               //   on the element start or element end tag.  Note that this
               //   simple method only works with data oriented documemts
               //   (no mixed content).
               if (reader.NodeType == XmlNodeType.Text) innerText = reader.Value;


               //   dump information about the element
               if (reader.NodeType == XmlNodeType.EndElement) {
                  describe(reader, innerText);
                  innerText = null;
               }
            }
         } catch (Exception e) {
            Console.WriteLine(e.Message);
         }
         Console.WriteLine();
         Console.WriteLine("Press Enter to Exit...");
         Console.ReadLine();
      }


      /// <summary>
      /// dump information about the current node and the related schema information
      /// </summary>
      /// <param name="reader">the XML reader validating the document</param>
      /// <param name="data">the data of the node; either the value of an
      /// attribute or the text of an element</param>
      private static void describe(XmlReader reader, string data) {
         if (reader.NodeType == XmlNodeType.EndElement || reader.NodeType == XmlNodeType.Attribute) {


            //   dump the type of element and its name
            Console.WriteLine(reader.NodeType.ToString() + ": " + reader.Name);


            //   dump the type of the node as it's known in the XSD type system
            string xmlDataType = getXmlDataType(reader);
            if (xmlDataType != null) Console.WriteLine("\t" + xmlDataType);


            //   dump the type of the node as it's known in the CLR type system
            //   and the node value as formatted by ToString() using the correct
            //   CLR type
            Type clrType = getClrType(reader);
            if (clrType != null) {
               Console.WriteLine("\t" + clrType.FullName);
               if (reader.NodeType == XmlNodeType.EndElement && data != null) {
                  Console.WriteLine("\t" + getTypedData(reader, data, clrType).ToString());
               } else if (reader.NodeType == XmlNodeType.Attribute) {
                  Console.WriteLine("\t" + getTypedData(reader, data, clrType).ToString());
               }
            }


            //   dump the schema appinfo associated with the node
            List<string> appInfo = getAppInfo(reader);
            if (appInfo != null && appInfo.Count > 0) {
               Console.WriteLine("\tAppInfo");
               foreach (string info in appInfo) Console.WriteLine("\t\t" + info);
            }


            //   dump the schema documentation associated with the node
            List<string> documentation = getDocumentation(reader);
            if (documentation != null && documentation.Count > 0) {
               Console.WriteLine("\tDocumentation");
               foreach (string doc in documentation) Console.WriteLine("\t\t" + doc);
            }


            //   dump all the regular expressions the restrict the node, if any
            List<string> patterns = getPattern(reader);
            if (patterns != null && patterns.Count > 0) {
               Console.WriteLine("\tPatterns");
               foreach (string pattern in patterns) Console.WriteLine("\t\t" + pattern);
            }


            //   dump the maximum length of the node, if specified
            List<int> maxLengths = getMaxLength(reader);
            if (maxLengths != null && maxLengths.Count > 0) {
               Console.WriteLine("\tMax Lengths");
               foreach (int max in maxLengths) Console.WriteLine("\t\t" + max.ToString());
            }
            Console.WriteLine();
         }
      }


      /// <summary>
      /// get the type of the node as a CLR datatype
      /// </summary>
      /// <param name="reader">the XML reader validating the document</param>
      /// <returns>CLR type</returns>
      private static Type getClrType(XmlReader reader) {
         if (reader.SchemaInfo.SchemaType == null || reader.SchemaInfo.SchemaType.Datatype == null) return null;
         return reader.SchemaInfo.SchemaType.Datatype.ValueType;
      }


      /// <summary>
      /// get the data of the node as a CLR type
      /// </summary>
      /// <param name="reader">the XML reader validating the document</param>
      /// <param name="data">the value of the node as it appears in the document</param>
      /// <param name="dataType">the CLR type of the node</param>
      /// <returns></returns>
      private static object getTypedData(XmlReader reader, object data, Type dataType) {
         if (reader.SchemaInfo.SchemaType == null) return null;
         return reader.SchemaInfo.SchemaType.Datatype.ChangeType(data, dataType);
      }


      /// <summary>
      /// get the appinfo information associated with the node
      /// </summary>
      /// <param name="reader">the XML reader validating the document</param>
      /// <returns>all the appinfo information</returns>
      private static List<string> getAppInfo(XmlReader reader) {
         XmlSchemaObjectCollection annotations = getAnnotations(reader);
         if (annotations == null) return null;
         List<string> list = new List<string>();
         foreach (XmlSchemaObject annotation in annotations) {
            if (annotation is XmlSchemaAppInfo) {
               foreach (XmlNode appInfo in ((XmlSchemaAppInfo) annotation).Markup) list.Add(appInfo.InnerText);
            }
         }
         return list; 
      }


      /// <summary>
      /// get the documentation information associated with the node
      /// </summary>
      /// <param name="reader">the XML reader validating the document</param>
      /// <returns>all the documentation information</returns>
      private static List<string> getDocumentation(XmlReader reader) {
         XmlSchemaObjectCollection annotations = getAnnotations(reader);
         if (annotations == null) return null;
         List<string> list = new List<string>();
         foreach (XmlSchemaObject annotation in annotations) {
            if (annotation is XmlSchemaDocumentation) {
               foreach (XmlNode doc in ((XmlSchemaDocumentation) annotation).Markup) list.Add(doc.InnerText);
            }
         }
         return list;
      }


      /// <summary>
      /// get all the annotations associated with a node
      /// </summary>
      /// <param name="reader">the XML reader validating the document</param>
      /// <returns>collection of annotations</returns>
      private static XmlSchemaObjectCollection getAnnotations(XmlReader reader) {
         XmlSchemaObjectCollection annotations = null;
         switch (reader.NodeType) {
            case XmlNodeType.EndElement:
               if (reader.SchemaInfo.SchemaElement == null) return annotations;
               combine(ref annotations, reader.SchemaInfo.SchemaElement.Annotation);
               if (reader.SchemaInfo.SchemaElement.ElementSchemaType == null) return annotations;
               combine(ref annotations, reader.SchemaInfo.SchemaElement.ElementSchemaType.Annotation);
               break;
            case XmlNodeType.Attribute:
               if (reader.SchemaInfo.SchemaAttribute == null) return annotations;
               combine(ref annotations, reader.SchemaInfo.SchemaAttribute.Annotation);
               if (reader.SchemaInfo.SchemaAttribute.AttributeSchemaType == null) return annotations;
               combine(ref annotations, reader.SchemaInfo.SchemaAttribute.AttributeSchemaType.Annotation);
               break;
            default:
               return null;
         }
         return annotations;
      }


      /// <summary>
      /// combine annotations from both the type definition and the node declaration
      /// </summary>
      /// <param name="collection">returned collection of annotations</param>
      /// <param name="annotations">collection of annotations to combine into first argument</param>
      private static void combine(ref XmlSchemaObjectCollection collection, XmlSchemaAnnotation annotations) {
         if (annotations == null) return;
         if (collection == null) collection = new XmlSchemaObjectCollection();
         foreach (XmlSchemaObject annotation in annotations.Items) collection.Add(annotation);
      }


      /// <summary>
      /// get all the restrictions associated with a node
      /// </summary>
      /// <param name="reader">the XML reader validating the document</param>
      /// <returns>collection of restrictions</returns>
      private static XmlSchemaSimpleTypeRestriction getRestriction(XmlReader reader) {
         XmlSchemaSimpleTypeRestriction restriction;
         XmlSchemaSimpleType simpleType;
         switch (reader.NodeType) {
            case XmlNodeType.EndElement:
               if (reader.SchemaInfo.SchemaElement == null) return null;
               simpleType = reader.SchemaInfo.SchemaElement.ElementSchemaType as XmlSchemaSimpleType;
               if (simpleType == null) return null;
               restriction = simpleType.Content as XmlSchemaSimpleTypeRestriction;
               break;
            case XmlNodeType.Attribute:
               if (reader.SchemaInfo.SchemaAttribute == null) return null;
               restriction = reader.SchemaInfo.SchemaAttribute.AttributeSchemaType.Content as XmlSchemaSimpleTypeRestriction;
               break;
            default:
               return null;
         }
         return restriction;
      }


      /// <summary>
      /// get all the regular expression patterns associated with a node, if any
      /// </summary>
      /// <param name="reader">the XML reader validating the document</param>
      /// <returns>list of regular expressions</returns>
      private static List<string> getPattern(XmlReader reader) {
         XmlSchemaSimpleTypeRestriction restriction = getRestriction(reader);
         if (restriction == null) return null;
         List<string> result = new List<string>();
         foreach (XmlSchemaObject facet in restriction.Facets) {
            if (facet is XmlSchemaPatternFacet) result.Add(((XmlSchemaFacet) facet).Value);
         }
         return result;
      }


      /// <summary>
      /// get the maximum length of a string, if specified
      /// </summary>
      /// <param name="reader">the XML reader validating the document</param>
      /// <returns>maximum length</returns>
      private static List<int> getMaxLength(XmlReader reader) {
         XmlSchemaSimpleTypeRestriction restriction = getRestriction(reader);
         if (restriction == null) return null;
         List<int> result = new List<int>();
         foreach (XmlSchemaObject facet in restriction.Facets) {
            if (facet is XmlSchemaMaxLengthFacet) result.Add(int.Parse(((XmlSchemaFacet) facet).Value));
         }
         return result;
      }


      /// <summary>
      /// get XML schema datatype of a node
      /// </summary>
      /// <param name="reader">the XML reader validating the document</param>
      /// <returns>XML schema data type</returns>
      private static string getXmlDataType(XmlReader reader) {
         if (reader.SchemaInfo.SchemaType == null) return null;
         return reader.SchemaInfo.SchemaType.TypeCode.ToString();
      }
   }
}

