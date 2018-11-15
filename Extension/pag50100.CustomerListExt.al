// Welcome to your new AL extension.
// Remember that object names and IDs should be unique across all extensions.
// AL snippets start with t*, like tpageext - give them a try and happy coding!

pageextension 50100 CustomerListExt extends "Customer List"
{

    actions
    {
        addlast(Creation)
        {
            group(BlobStorage)
            {
                Action(Upload)
                {
                    ApplicationArea = All;
                    Caption = 'Upload to blob Storage';

                    trigger OnAction();
                    var
                        blobStorageMgt: Codeunit BlobStorageMgt;
                    begin
                        blobStorageMgt.UploadFile();
                    end;
                }

                Action(Download)
                {
                    ApplicationArea = All;
                    Caption = 'Download from blob Storage';

                    trigger OnAction();
                    var
                        blobStorageMgt: Codeunit BlobStorageMgt;
                    begin
                        blobStorageMgt.DownloadFile('TBC.png', 'https://XXX.blob.core.windows.net:443/files/');
                    end;
                }
            }
        }
    }
}
