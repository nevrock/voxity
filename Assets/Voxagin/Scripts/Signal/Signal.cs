namespace Ngin {
    using UnityEngine;
    using System.Collections.Generic;
    public class Signal {
        List<Task> _tasks;  
        public Signal() {
            _tasks = new List<Task>();
        }
        public void AddListener(Task t) {
            _tasks.Add(t);
        }
        public void Execute() {
            foreach (Task t in _tasks) {
                t.Execute();
            }
        }
    }
}