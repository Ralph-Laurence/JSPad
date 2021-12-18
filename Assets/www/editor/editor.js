ace.require("ace/ext/language_tools"); 
var aceEditor = ace.edit("editor");

var editor_changed = false;

// Setup searchbox
ace.config.loadModule("ace/ext/searchbox", function(m) 
{
    // Bind searchbox to editor
    m.Search(aceEditor);

    // Hide searchbar onload
    aceEditor.searchBox.hide();
});

aceEditor.setOptions
({
    enableBasicAutocompletion: true,
    enableSnippets: true,
    enableLiveAutocompletion: true,
    showPrintMargin: false,
    useSoftTabs: false,
    fontSize: 12,
    showInvisibles: false, 
    tabSize: 4,  
    showLineNumbers: true, 
    highlightActiveLine: true, 
    highlightSelectedWord: true,
    displayIndentGuides: true,
    showFoldWidgets: true,
    newLineMode: 'auto', 
    foldStyle: 'markbeginend',
    
});  

// defines the style of the editor
aceEditor.setTheme("ace/theme/dark");  

// ensures proper autocomplete, validation and highlighting of JavaScript code
aceEditor.getSession().setMode("ace/mode/javascript");

// Watch for change events
aceEditor.getSession().on("change", () => editor_changed = true);

// Setup Undo Manager
var UndoManager = ace.require("ace/undomanager").UndoManager;
var undoManager = new UndoManager();

aceEditor.getSession().setUndoManager(this.undoManager);

//-------------------------------
// Common Edit functions
//-------------------------------

// virtual clipboard that holds strings of copied texts
var virtualClipboard = "";

function InsertTab() 
{  
    aceEditor.insert("\u0009");
}
  
function Undo() 
{
    aceEditor.session.getUndoManager().undo();
}

function Redo() 
{ 
    aceEditor.session.getUndoManager().redo();
}

function GetValue() 
{ 
    return aceEditor.getValue();
}

function SetValue(code) 
{ 
    aceEditor.getSession().setValue(code);
}

function Find() 
{ 
    aceEditor.searchBox.show();
}

function Copy()
{
    var response = false;

    try
    {
        aceEditor.execCommand("copy");
        virtualClipboard = aceEditor.getCopyText();
    } catch (e) { response = false; }

    return response;
}

function Cut() 
{ 
    Copy();
    aceEditor.insert("");
}

function Paste() 
{  
    aceEditor.insert(virtualClipboard);
}

function SelectAll() 
{  
    aceEditor.selectAll();
}

function ClearEditor() 
{
    SelectAll();
     
    SetValue("");
}

function GetEditorChanged() 
{
    return editor_changed;
}

function SetEditorUnChangedState() 
{  
    editor_changed = false;
}