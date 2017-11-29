using Codeless;
using Microsoft.Web.Administration;
using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Hosting;

namespace System.Web {
  /// <summary>
  /// Maps document extensions to content MIME types.
  /// </summary>
  public static class MimeMapping {
    private static readonly ConcurrentFactory<string, Hashtable> cache = new ConcurrentFactory<string, Hashtable>();

    /// <summary>
    /// Returns the MIME mapping for the specified file name.
    /// </summary>
    /// <param name="filename">The file name that is used to determine the MIME type.</param>
    /// <returns></returns>
    public static string GetMimeMapping(string filename) {
      CommonHelper.ConfirmNotNull(filename, "filename");
      if (HostingEnvironment.IsHosted) {
        string siteName = HostingEnvironment.ApplicationHost.GetSiteName();
        Hashtable ht = cache.GetInstance(siteName, LoadMimeMappings);
        string value = (string)ht[Path.GetExtension(filename)];
        if (value != null) {
          return value;
        }
      }
      object entry = Registry.GetValue("HKEY_CLASSES_ROOT\\" + Path.GetExtension(filename), "Content Type", null);
      if (entry != null) {
        return entry.ToString();
      }
      return "application/octet-stream";
    }

    private static Hashtable LoadMimeMappings(string arg) {
      Hashtable ht = new Hashtable();
      using (HostingEnvironment.Impersonate()) {
        using (ServerManager serverManager = new ServerManager()) {
          Microsoft.Web.Administration.Configuration config = serverManager.GetWebConfiguration(arg);
          ConfigurationSection staticContentSection = config.GetSection("system.webServer/staticContent");
          foreach (ConfigurationElement elm in staticContentSection.GetCollection()) {
            object attrExt = elm.GetAttributeValue("fileExtension");
            object attrMimeType = elm.GetAttributeValue("mimeType");
            if (attrExt != null && attrMimeType != null) {
              ht[attrExt.ToString()] = attrMimeType.ToString().Split(';')[0];
            }
          }
        }
      }
      return ht;
    }
  }
}
