using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Xml;
using System.Xml.Schema;
using System.IO;
using System.Xml.Linq;

namespace Tradefast
{
  class Program
  {

    static List<XmlReader> errorElements = new List<XmlReader>();
    static List<String> errorElementsNames = new List<String>();
    static String name;
    static String innerText;
    static void Main(string[] args)
    {
      // Create the XmlSchemaSet class.
      XmlSchemaSet sc = new XmlSchemaSet();
      
      // Add the schema to the collection.
      sc.Add("urn:temp-schema", "temp.xsd");

      // Set the validation settings.
      XmlReaderSettings settings = new XmlReaderSettings();
      settings.ValidationType = ValidationType.Schema;
      settings.Schemas = sc;
      settings.ValidationEventHandler += new ValidationEventHandler(ValidationCallBack);

      // Create the XmlReader object.
      XmlReader reader = XmlReader.Create("temp.xml", settings);

      
      // Parse the file. 
      while (reader.Read())
      {
        //if (reader.NodeType == XmlNodeType.Element) name = reader.Name;
        ////   Capture the an elements text.  This is necessary because
        ////   the schema information isn't available when the reader is
        ////   on the text node; it is available only when the reader is
        ////   on the element start or element end tag.  Note that this
        ////   simple method only works with data oriented documemts
        ////   (no mixed content).
        //if (reader.NodeType == XmlNodeType.Text) innerText = reader.Value;

      }

      //// Load the XDocument from the reader
      //XDocument loadedDoc = XDocument.Load(reader);
      Console.WriteLine("ElementName", errorElements[0].Name);
      Console.WriteLine("ElementName", errorElements[1].Name);
    }

    // Display any validation errors.
    // The invalid node is the event handler's "sender" itself.
    private static void ValidationCallBack(object sender, ValidationEventArgs args)
    {
      Console.WriteLine("Validation Error: {0}", args.Message);

      var exception = (args.Exception as XmlSchemaValidationException);
      if (exception != null)
      {
        if (sender != null)
        {
          var element = sender as XmlReader;
          errorElements.Add(element);
          errorElementsNames.Add(element.Name);
        }

      }
    }

  }
}
