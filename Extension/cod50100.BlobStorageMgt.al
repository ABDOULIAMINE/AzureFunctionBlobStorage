codeunit 50100 BlobStorageMgt
{
    trigger OnRun()
    begin

    end;

    procedure UploadFile()
    var
        fileMgt: Codeunit "File Management";
        selectedFile: Text;
        tempblob: Record TempBlob;
        httpClient: HttpClient;
        httpContent: HttpContent;
        jsonBody: text;
        httpResponse: HttpResponseMessage;
        httpHeader: HttpHeaders;
    begin
        selectedFile := fileMgt.OpenFileDialog('NAV File Browser', '', '');
        tempblob.Blob.Import(selectedFile);

        jsonBody := ' {"base64":"' + GetMimeType(selectedFile) + ';base64,' + tempblob.ToBase64String() +
        '","fileName":"' + delchr(fileMgt.GetFileName(selectedFile), '=', '.' + fileMgt.GetExtension(selectedFile)) +
        '","fileType":"' + fileMgt.GetExtension(selectedFile) + '"}';

        httpContent.WriteFrom(jsonBody);
        httpContent.GetHeaders(httpHeader);
        httpHeader.Remove('Content-Type');
        httpHeader.Add('Content-Type', 'application/json');
        httpClient.Post('XXX/api/UploadFile', httpContent, httpResponse);
    end;

    procedure DownloadFile(fileName: Text; blobUrl: Text)
    var
        tempblob: Record TempBlob;
        httpClient: HttpClient;
        httpContent: HttpContent;
        jsonBody: text;
        httpResponse: HttpResponseMessage;
        httpHeader: HttpHeaders;
        base64: Text;

    begin
        jsonBody := ' {"url":"' + blobUrl + '","fileName":"' + fileName + '"}';
        httpContent.WriteFrom(jsonBody);
        httpContent.GetHeaders(httpHeader);
        httpHeader.Remove('Content-Type');
        httpHeader.Add('Content-Type', 'application/json');
        httpClient.Post('XXX/api/DownloadFile', httpContent, httpResponse);
        httpResponse.Content.ReadAs(base64);
        base64 := DelChr(base64, '=', '"');
        tempblob.FromBase64String(base64);
    end;

    local procedure GetMimeType(selectedFile: Text): Text
    var
        fileMgt: Codeunit "File Management";
        mimeType: Text;
    begin
        case lowercase(fileMgt.GetExtension(selectedFile)) of
            'pdf':
                mimeType := 'data:application/pdf';
            'txt':
                mimeType := 'data:text/plain';
            'png':
                mimeType := 'data:image/png';
            else
                Error('File Format Not Supported');
        end;
        EXIT(mimeType);
    end;
}