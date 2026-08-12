# TinyUrl Service — Architecture & Caching Strategy

**Repository pointers**
- `Services/UrlService.cs`: main URL creation & lookup flow
- `Utils/SqidsGenerator.cs`: transforms global numeric IDs into compact, collision-resistant strings
- `Data/Redis.cs`: cache access layer
- `Data/Postgres.cs`: durable storage layer

Overview
--------
TinyUrl implements a deterministic, high-throughput short URL generator with strong cache usage. The short ID lifecycle is:

1. Allocate a unique numeric ID from a global, atomic counter.
2. Encode that numeric ID with the SQIDs generator to produce a compact short ID.
3. Persist the mapping (short ID -> full URL) in Postgres.
4. Insert the same mapping into Redis so lookups hit the cache first.

This approach ensures no collisions (IDs are unique by construction), short, human-friendly tokens (via SQIDs), and high read throughput (via Redis cache).

Why a global counter + SQIDs
----------------------------
- Global counter (atomic increment) gives a monotonic, unique numeric ID — it's simple, fast, and avoids costly uniqueness checks in the database.
- SQIDs (`SqidsGenerator.cs`) deterministically encodes the numeric ID into a short, URL-safe string using a salt/parameter set. Because we encode the globally-unique numeric ID, collisions in the output space are impossible while the encoding parameters remain stable.
- This combination is preferable to randomized short tokens when you want predictable size, no retries for collisions, and simple sharding strategies.

Cache-first lookup flow
-----------------------
1. Client requests a short URL lookup.
2. Service queries Redis (`Data/Redis.cs`) for the short ID.
   - If found: return mapped URL (very low latency).
   - If missing: fall back to Postgres (`Data/Postgres.cs`).
       - If Postgres contains the mapping: return it and write it back to Redis (populate cache on miss).
       - If Postgres does not contain the mapping: return 404.

Cache-on-write (creation) flow
------------------------------
When creating a new short URL:
1. Atomically increment global counter (e.g., DB sequence, Redis INCR, or a centralized counter service).
2. Encode numeric ID with `SqidsGenerator.cs` to produce short ID.
3. Persist mapping in Postgres within a transaction.
4. Write mapping into Redis immediately after successful DB commit. This makes the cache consistent and avoids initial cache misses for just-created links.

Consistency and failure handling
--------------------------------
- Atomicity: counter increment + DB insert should be arranged so that a failed DB insert does not leave a permanently consumed ID if that matters for your business constraints. Options:
  - Accept occasional holes in the numeric sequence (common, simple): increment first, if DB insert fails, the numeric ID is lost but no collision occurs.
  - Two-phase allocation or transactional counters if sequence gaps are unacceptable.
- Cache population: write-through-on-success ensures the cache reflects the DB state after creation. On cache write failures, the system still has durable data in Postgres and will repopulate the cache on the first miss.
- Race conditions on cache miss: concurrent cache-miss lookups for the same short ID may cause multiple DB reads and repeated cache writes. Mitigations:
  - Use a short lock (distributed lock) per-key to limit thundering herds.
  - Use double-checked locking: read cache -> if miss then DB -> set cache if value exists, but tolerate duplicate sets.

Cache configuration and policies
------------------------------
- TTL: choose a reasonable TTL (e.g., days/weeks depending on URL lifetime). Long TTLs maximize hit rate; short TTLs allow faster invalidation after updates.
- Eviction: Redis LRU or size-based eviction keeps memory bounded.
- Persistence: Redis is a cache; Postgres is the source of truth. On full cache restart, the application gracefully falls back to DB and repopulates.

Scalability and operations
--------------------------
- Read scale: Redis handles very high read QPS; keep read path cache-first to minimize DB load.
- Write scale: global counter allocation can be a bottleneck at extreme write QPS. Strategies:
  - Use sharded counters (allocate ranges per instance) and encode range offset into SQIDs.
  - Use Redis INCR or DB sequences with batching to reduce coordination overhead.
- Observability: track metrics for cache hit rate, cache misses per second, DB latency, counter allocation latency, and number of holes (if tracking lost IDs).
- Backups & recovery: Postgres backups are the canonical source; Redis backup/replication helps fast warm restarts but is not required for correctness.

