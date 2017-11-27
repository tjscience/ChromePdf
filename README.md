# ChromePdf

ChromePdf is a command line utility that simplifies the process of generating PDFs from HTML. 
ChromePdf handles this by using [CefSharp](https://github.com/cefsharp/CefSharp) to render the HTML and generate the PDF. 
As you may know, generating accurate PDFs from HTML from a backend application or even from the command line can be very cumbersome. 
There are many 3rd party libraries out there but many of them cost money and tend not to be very accurate. 
There are also some open source libraries, however, I found that many of them were either not complete, no longer being supported, or they 
did not create accurate PDFs. 

So, I decided to leverage chromium to do all of the heavy lifiting because I have found that it 
produces the most accurate PDFs when generating them from HTML. I decided to share this utility in case anyone else might need it. Hence, 
ChromePdf!

The utility itself is just a windows console app which takes several arguments, so it is pretty straight-forward to use. To see help for 
the utility, simply call it from the command line without any arguments `chromepdf`. Here are the help contents for reference:

```
ChromePdf 1.0.0.0
Copyright c  2017

  -i, --input                   Required. Input URI (required).

  -o, --output                  Required. Output file path (required).

  -l, --landscape               (Default: False) Specifies that the PDF should
                                be rendered in landscape orientation.

  -b, --enable-backgrounds      (Default: False) Specifies that background
                                graphics should be rendered in the PDF.

  -n, --timeout                 (Default: 0) Specifies a timeout (milliseconds)
                                in which to kill the process if it has not
                                exited successfully (0 = no timeout).

  -d, --delay                   (Default: 0) Specifies a delay (milliseconds)
                                in which to wait for the URI to load before
                                processing (0 = no delay; This is independent
                                on the time it takes the page to load in the
                                browser.).

  -m, --margin-type             (Default: Default) Specifies the margin for the
                                PDF (available values: Custom, Default,
                                Minimum, None).

  -h, --enable-header-footer    (Default: False) Specifies that the
                                header/footer should be rendered in the PDF.

  -t, --title                   A title to be rendered in the PDF header (only
                                used if enable-header-footer present).

  -f, --footer-text             Text to be rendered in the PDF footer (only
                                used if enable-header-footer present).

  --margin-bottom               (Default: 0) Bottom margin (only used if
                                margin-type is set to custom).

  --margin-left                 (Default: 0) Left margin (only used if
                                margin-type is set to custom).

  --margin-right                (Default: 0) Right margin (only used if
                                margin-type is set to custom).

  --margin-top                  (Default: 0) Top margin (only used if
                                margin-type is set to custom).

  --page-height                 (Default: 0) Page height in microns (if 0 paper
                                size is set to A4).

  --page-width                  (Default: 0) Page width in microns (if 0 paper
                                size is set to A4).                               
                                
```

## Some Simple Examples 

Generate a PDF of github.com in landscape mode with background graphics:
```
chromepdf -i "https://github.com" -o github.pdf -l -b
```

Generate a PDF of a local html file:
```
chromepdf -i "<path to file>\test.html" -o github.pdf -l -b
```







