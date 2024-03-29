# LargeEDIFileReader
WPF desktop application specifically designed to read large X12 EDI files that do not have line breaks.
This tool is very specific to the healthcare industry, as that filetype is mandated when exchanging data, per HIPAA federal regulations.

### Current working features
* Parse and read very large files quickly and with little memory footprint.
* Search the entire file and display a list of results.
* Navigate through the file via 10,000 segment "pages".


### Roadmap
* Add Drag-and-Drop option to open files.
* ~~Line numbers in main file view.~~ DONE!
* ~~Double-click search results to jump to that segment on that page.~~ DONE!
* ~~Go to a specific segment offset/line.~~ DONE!
* ~~Change cursor to "waiting" during searches.~~ DONE!
* ~~Change cursor to "waiting" when scanning the file after opening, and building the page offsets.~~ DONE!
* ~~Highlight line when jumping to segment~~ DONE!

