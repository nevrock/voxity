namespace Ngin
{
    using UnityEngine;
    using System.Collections.Generic;
    using System.Collections;


    public class RoutineWorker : MonoBehaviour
    {
        public static int _counter;

        public static RoutineWorker Spawn(string routineName, Lexicon args) {
            RoutineWorker worker = New();

            Log.Console($"Routine worker spawn! - {routineName}, {_counter}");

            switch (routineName)
            {
                case "delay":
                    worker.InvokeTask(args.Get<Task>("task", null),
                        args.Get<float>("time_delay", 0.1f));
                    return worker;
                case "wait":
                    worker.InvokeWait(args.Get<Task<int, bool>>("task_wait_condition", null),
                        args.Get<Task>("task_when_complete", null));
                    return worker;
                case "routine":
                    worker.InvokeRoutine(args.Get<IEnumerator>("routine"));
                    return worker;
            }

            // if we are here, destroy
            Destroy(worker.gameObject);
            return null;
        }
        public static RoutineWorker Spawn<T>(string routineName, Lexicon args) {
            RoutineWorker worker = New();
            
            switch (routineName)
            {
                case "load":
                    worker.InvokeLoad<T>(
                        source:args.Get<IEnumerable<T>>("source_collection", null),
                        actionCallback:args.Get<Task<T>>("task_when_callback", null),
                        counter:args.Get<int>("counter", 200),
                        actionOnEnd:args.Get<Task>("task_when_end", null));

                    return worker;

                case "lerp":
                    worker.InvokeLerp<T>(
                        args.Get<string>("lerp_type", "point"),
                        args.Get<T>("val_in", default(T)),
                        args.Get<T>("val_out", default(T)),
                        args.Get<float>("time", 1f),
                        args.Get<Task<T>>("task_when_callback", null),
                        args.Get<Task>("task_when_end", null));

                    return worker;
            }

            // if we are here, destroy
            Destroy(worker.gameObject);
            return null;
        }
        public static RoutineWorker New() {
            _counter++;
            GameObject o = new GameObject("worker_" + _counter.ToString());
            RoutineWorker worker = o.AddComponent<RoutineWorker>();
            return worker;
        }

        Coroutine _routine;

        public void Stop() {
            if (_routine != null) {
                StopCoroutine(_routine);
            }
            DestroyImmediate(this.gameObject);
        }
        
        public void InvokeRoutine(IEnumerator routine) {
            _routine = StartCoroutine(routine);
        }
        public void InvokeTask(Task actionOnDelay, float time) {
            _routine = StartCoroutine(RoutineInvoke(actionOnDelay, time));
        }
        public void InvokeWait(
            Task<int, bool> actionWaitCondition, Task actionOnComplete
        ) {
            _routine = StartCoroutine(RoutineWait(actionWaitCondition, actionOnComplete));
        }
        public void InvokeLoad<T>(
            IEnumerable<T> source, Task<T> actionCallback, int counter = 200, Task actionOnEnd = null
        ) {
            _routine = StartCoroutine(RoutineLoad<T>(
                source, actionCallback, counter, actionOnEnd
            ));
        }  
        public void InvokeLerp<T>(string name, T valIn, T valOut, float time = 1f, Task<T> taskCallback = null, Task taskWhenEnd = null) {
            if (!this.gameObject.activeInHierarchy)
            {
                if (taskCallback != null)
                    taskCallback.Execute(valOut);
                return;
            }
            TaskBase taskCallbackLerp = null;

            switch (name)
            {
                case "float":
                    taskCallbackLerp = new Task<float,float,float,float>(
                        (x, y, z) => {
                            return Mathf.Lerp(x, y, z);
                        }
                    );
                    break;
                case "vec":
                    taskCallbackLerp = new Task<Vector3,Vector3,float,Vector3>(
                        (x, y, z) => {
                            return Vector3.Lerp(x, y, z);
                        }
                    );
                    break;
                case "point":
                    taskCallbackLerp = new Task<TransformData,TransformData,float,TransformData>(
                        (x, y, z) => {
                            return x.Lerp(y, z);
                        }
                    );
                    break;
                case "rotation":
                    taskCallbackLerp = new Task<Quaternion,Quaternion,float,Quaternion>(
                        (x, y, z) => {
                            return Quaternion.Lerp(x, y, z);
                        }
                    );
                    break;
                case "text":
                    taskCallbackLerp = new Task<string,string,float,string>(
                        (x,y,z) => {
                            char[] chars = y.ToCharArray();
                            float countFloat = ((float)(chars.Length))*z;
                            int count = (int)countFloat;
                            string textOut = "";
                            int i = 0;
                            foreach (char c in chars)
                            {
                                textOut+=c.ToString();
                                i++;
                                if (i > count)
                                    break;
                            }

                            return textOut;
                        }
                    );
                    break;
            }
            if (taskCallbackLerp == null)
                return;


            Coroutine routineLoad = StartCoroutine(RoutineLerp<T>(valIn, valOut, taskCallbackLerp, time, taskCallback, taskWhenEnd));

            _routine = routineLoad;
        } 
        private void InvokeLerp<T>(T valIn, T valOut, TaskBase taskCallbackLerp, float time = 1f, Task<T> taskCallback = null) {
            if (!this.gameObject.activeInHierarchy)
            {
                if (taskCallback != null)
                    taskCallback.Execute(valOut);
                return;
            }
            if (taskCallbackLerp == null)
                return;

            Coroutine routineLoad = StartCoroutine(RoutineLerp<T>(valIn, valOut, taskCallbackLerp, time, taskCallback));

            _routine = routineLoad;
        } 

        private IEnumerator RoutineInvoke(Task actionOnDelay, float time) {
            Log.Console($"Routine worker invoke task, with delay time: {time}");

            yield return new WaitForSeconds(time);
            
            if (actionOnDelay != null)
                actionOnDelay.Execute();

            Destroy(this.gameObject);
        }
        private IEnumerator RoutineLoad<T>(IEnumerable<T> source, Task<T> actionCallback, int counter = 200, Task actionOnEnd = null) {
            yield return null;
            
            int i = 0;
            foreach (T val in source)
            {
                actionCallback.Execute(val);
                i++;
                if (i%counter == 0)
                {
                    yield return null;
                }
            }

            if (actionOnEnd != null)
                actionOnEnd.Execute();

            Destroy(this.gameObject);
        }
        private IEnumerator RoutineWait(Task<int, bool> actionWaitCondition, Task actionOnComplete) {
            while (!actionWaitCondition.Execute(0))
            {
                yield return null;
            }   
            actionOnComplete.Execute();

            Destroy(this.gameObject);
        }
        private IEnumerator RoutineLerp<T>(T valIn, T valOut, TaskBase taskCallbackLerp, float time = 1f, Task<T> taskCallback = null, Task taskWhenComplete = null) {
            yield return null;

            Task<T,T,float,T> lerp_task = taskCallbackLerp as Task<T,T,float,T>;
            if (lerp_task == null)
                yield break;

            float t = 0f;
            if (taskCallback != null)
                taskCallback.Execute(valIn);

            while (t <= 1.0f)
            {   
                T val = lerp_task.Execute(valIn, valOut, t);

                if (taskCallback != null)
                    taskCallback.Execute(val);

                t += Time.deltaTime/time;
                yield return null;
            }

            if (taskCallback != null)
                taskCallback.Execute(valOut);

            if (taskWhenComplete != null)
                taskWhenComplete.Execute();

            Destroy(this.gameObject);
        }
    }
}