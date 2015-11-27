FastCache
=========

Fast Cache for Umbraco

v2.0 (Tim Scarfe)

* Partially re-written and cleaned up. There was a lot of code cruft and debug writes etc (looks like the previous developers couldn't step through it in a debugger?)
* Removed the "spider" component and aspx files
* Config strings are now in a resource file
* The events code now works on recent Umbraco versions (using the 6+ api) and I have tested it deleting files from the cache after a publish
* As well as caching to the file system it also caches the html into the application cache and doesn't even touch the file system after the initial check. This speeds it up a lot especially on Azure-hosted sites because file system access is slower than on a native box.