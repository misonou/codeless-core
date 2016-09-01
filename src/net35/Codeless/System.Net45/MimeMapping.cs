using Codeless;
using Microsoft.Web.Administration;
using Microsoft.Win32;
using System;
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
    /// <summary>
    /// Returns the MIME mapping for the specified file name.
    /// </summary>
    /// <param name="filename">The file name that is used to determine the MIME type.</param>
    /// <returns></returns>
    public static string GetMimeMapping(string filename) {
      CommonHelper.ConfirmNotNull(filename, "filename");
      object entry = Registry.GetValue("HKEY_CLASSES_ROOT\\" + Path.GetExtension(filename), "Content Type", null);
      if (entry != null) {
        return entry.ToString();
      }
      using (HostingEnvironment.Impersonate()) {
        using (ServerManager serverManager = new ServerManager()) {
          string siteName = HostingEnvironment.ApplicationHost.GetSiteName();
          Microsoft.Web.Administration.Configuration config = serverManager.GetWebConfiguration(siteName);
          ConfigurationSection staticContentSection = config.GetSection("system.webServer/staticContent");
          ConfigurationElementCollection staticContentCollection = staticContentSection.GetCollection();
          ConfigurationElement mimeMap = staticContentCollection.FirstOrDefault(v => v.GetAttributeValue("fileExtension") != null && v.GetAttributeValue("fileExtension").ToString() == Path.GetExtension(filename));
          if (mimeMap != null) {
            return mimeMap.GetAttributeValue("mimeType").ToString().Split(';')[0];
          }
        }
      }
      return "application/octet-stream";
    }
  }
}
