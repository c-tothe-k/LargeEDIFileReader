# LargeEDIFileReader
WPF desktop application specifically designed to read large X12 EDI files with no line breaks

### Current working features
* Parse and read very large files quickly and with little memory footprint.
* Search the entire file and display a list of results.
* Navigate through the file via 10,000 segment "pages".

### Roadmap
* Line numbers in main file view.
* Double-click search results to jump to that segment on that page.
* Go to a specific segment offset/line.
* Change cursor to "waiting" when scanning the file after opening, and building the page offsets.
* Change cursor to "waiting" during searches.
