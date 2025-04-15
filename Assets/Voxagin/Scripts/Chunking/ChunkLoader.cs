using UnityEngine;
using Unity.Jobs;
using Unity.Collections;
using System.Collections.Generic;

namespace Ngin {

    public class ChunkLoader : MonoBehaviour
    {
        public Transform player;
        public int chunkSize = 16;
        public int viewDistance = 5;

        private NativeArray<Vector2Int> chunkPositions;
        private NativeArray<bool> chunkLoadResults;
        private Dictionary<Vector2Int, Chunk> loadedChunks = new Dictionary<Vector2Int, Chunk>();

        void Start()
        {
            int totalChunks = (viewDistance * 2 + 1) * (viewDistance * 2 + 1);
            chunkPositions = new NativeArray<Vector2Int>(totalChunks, Allocator.Persistent);
            chunkLoadResults = new NativeArray<bool>(totalChunks, Allocator.Persistent);
        }

        void Update()
        {
            ScheduleChunkLoadingJob();
        }

        void ScheduleChunkLoadingJob()
        {
            Vector2Int playerChunkPos = new Vector2Int(
                Mathf.FloorToInt(player.position.x / chunkSize),
                Mathf.FloorToInt(player.position.z / chunkSize)
            );

            int index = 0;
            for (int x = -viewDistance; x <= viewDistance; x++)
            {
                for (int z = -viewDistance; z <= viewDistance; z++)
                {
                    chunkPositions[index] = new Vector2Int(playerChunkPos.x + x, playerChunkPos.y + z);
                    index++;
                }
            }

            ChunkLoadingJob chunkLoadingJob = new ChunkLoadingJob
            {
                chunkPositions = chunkPositions,
                chunkLoadResults = chunkLoadResults,
                playerChunkPos = playerChunkPos,
                viewDistance = viewDistance
            };

            JobHandle jobHandle = chunkLoadingJob.Schedule(chunkPositions.Length, 64);
            jobHandle.Complete();

            for (int i = 0; i < chunkLoadResults.Length; i++)
            {
                Vector2Int chunkPos = chunkPositions[i];
                if (chunkLoadResults[i])
                {
                    if (!loadedChunks.ContainsKey(chunkPos))
                    {
                        LoadChunk(chunkPos);
                    }
                }
                else
                {
                    if (loadedChunks.ContainsKey(chunkPos))
                    {
                        UnloadChunk(chunkPos);
                    }
                }
            }
        }

        void LoadChunk(Vector2Int chunkPos)
        {
            Chunk chunk = new Chunk(chunkPos);
            chunk.Load();
            loadedChunks.Add(chunkPos, chunk);
        }

        void UnloadChunk(Vector2Int chunkPos)
        {
            if (loadedChunks.TryGetValue(chunkPos, out Chunk chunk))
            {
                chunk.Unload();
                loadedChunks.Remove(chunkPos);
            }
        }

        void OnDestroy()
        {
            chunkPositions.Dispose();
            chunkLoadResults.Dispose();
        }

        struct ChunkLoadingJob : IJobParallelFor
        {
            [ReadOnly] public NativeArray<Vector2Int> chunkPositions;
            public NativeArray<bool> chunkLoadResults;
            public Vector2Int playerChunkPos;
            public int viewDistance;

            public void Execute(int index)
            {
                Vector2Int chunkPos = chunkPositions[index];
                float distance = Vector2Int.Distance(chunkPos, playerChunkPos);
                chunkLoadResults[index] = distance <= viewDistance;
            }
        }
    }

    public class Chunk
    {
        public Vector2Int chunkPosition;
        private GameObject chunkObject;

        public Chunk(Vector2Int position)
        {
            chunkPosition = position;
        }

        public void Load()
        {
            chunkObject = new GameObject($"Chunk_{chunkPosition.x}_{chunkPosition.y}");
            // Additional loading logic here
        }

        public void Unload()
        {
            if (chunkObject != null)
            {
                GameObject.Destroy(chunkObject);
                chunkObject = null;
            }
            // Additional unloading logic here
        }
    }
}
