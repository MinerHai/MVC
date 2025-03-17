namespace App.Models{
    public class Summernote{
        public Summernote(string? _IDEditor, bool _LoadLib = true){
            IDEditor = _IDEditor;
            LoadLib = _LoadLib;
        }
        public string? IDEditor { get; set; }
        public bool LoadLib {get; set;}
        public int height {get; set;} = 120;
        public string toolbar {get; set;} = @" 
                [['style', ['style']],
                ['font', ['bold', 'underline', 'clear']],
                ['color', ['color']],
                ['para', ['ul', 'ol', 'paragraph']],
                ['table', ['table']],
                ['insert', ['link', 'picture', 'video']],
                ['view', ['fullscreen', 'codeview', 'help']]
                ";        
    }
 
}