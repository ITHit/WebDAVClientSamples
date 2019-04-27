# WebDAV Client iOS Sample in C#, Xamarin

WebDAV Client iOS Sample in C#, Xamarin
This sample is an iOS file provider application that runs in the background and processes requests from iOS file system API. You can use any iOS application, such as MS Word, to browse, open documents from your WebDAV server, edit and save document back to server. You can use iOS Files application to navigate file structure, upload, copy, move and delete documents. Typically you will customize this sample to your needs to open documents from your DMS/CRM/ERP system.


## Solution Structure 
The iOS sample solution consists of 3 projects: container application, an extension project and a common code.

The container application provides UI to enter WebDAV server URL and user credentials. You can modify this application to adapt to your needs. Typically, you will hardcode the WebDAV server URL, implement required authentication and customize UI used for connecting to your server.

The extension project runs in the background and implements a file system on iOS (file provider). It processes requests from iOS applications sent via iOS file system API, reads and writes files, lists folders content, copies, moves, locks and unlocks documents. The iOS extension can be installed only as part of container application, you can not install the container application by itself.

[A detailed instructions about how to run this sample could be found here](https://www.webdavsystem.com/client/examples/ios_xamarin/).