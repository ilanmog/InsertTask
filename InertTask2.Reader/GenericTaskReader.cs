using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace InertTask2.Reader {
    public class GenericTaskReader : IGenericTaskReader {
        StringBuilder sb = null;
        private static Regex _tasksRegex = new Regex(@"^\t(?<identifier>(?<completed>[\+])?(?<outside>[X])?(?<category>[ABCD])[TJ]?[0-9\.]+)\:\:(?<title>.*)$", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.Multiline);
        private string _tasksContent;
        private HashSet<string> _actualFiles;
        public GenericTaskReader(string tasksContent, string[] actualFiles) {
            _tasksContent = tasksContent;
            _actualFiles = new HashSet<string>();
            foreach (var file in actualFiles) {
                _actualFiles.Add(file);
            }
        }
        public ICollection<GenericTask> GetTasks() {
            return _tasksRegex.Matches(_tasksContent).Cast<Match>().Select(m => {
                var identifier = m.Groups["identifier"].Value;
                return new GenericTask() {
                    Identifier = identifier,
                    IsCompleted = m.Groups["completed"].Success,
                    IsOutside = m.Groups["outside"].Success && _actualFiles.Contains(identifier + ".txt"),
                    Title = m.Groups["title"].Value,
                    Category = m.Groups["category"].Value
                };
            }).ToList();
        }

        private List<NoteLine> GetDocumentLines() {
        var lines = sb.ToString().Split(new string[] { "\r\n" }, StringSplitOptions.None);
        int i = 0;
        int lineIndex = 0;
        List<NoteLine> values = new List<NoteLine>();
        foreach (string line in lines) {
            values.Add(new NoteLine() {
                FromIndex = i,
                ToIndex = line.Length + i,
                Line = line,
                LineIndex = lineIndex++
            });
            i += line.Length + 2;
        }
        return values;
    }

    }
}
