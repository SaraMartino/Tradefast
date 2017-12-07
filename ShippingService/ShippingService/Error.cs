using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShippingService
{
  class Error
  {
    private static Dictionary<int, String> errorsDescription;
    int code;
    String message;

    static Error()
    {
      errorsDescription = new Dictionary<int, string>();
      errorsDescription.Add(0, "Unspecified error");
      errorsDescription.Add(1, "Error 1");
      errorsDescription.Add(2, "Error 2");
      errorsDescription.Add(3, "Error 3");
      errorsDescription.Add(4, "Error 4");
      errorsDescription.Add(5, "Error 5");
    }

    /// <summary>
    /// Construct an object of type Error setting code and message depending on the code passed as input.
    /// If the input is not a valid code, the default code is 0 (Unspecified Error).
    /// </summary>
    public Error(int errorCode)
    {
      // code = (errorsDescription.ContainsKey(_code)) ? _code : 0;
      if (errorsDescription.ContainsKey(errorCode))
        code = errorCode;
      else
        code = 0;

      message = errorsDescription[code];
    }

    public int getCode()
    {
      return code;
    }

    public String getMessage()
    {
      return message;
    }
  }
}
