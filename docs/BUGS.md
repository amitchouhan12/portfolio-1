# Five deliberately planted production-style bugs

The exercise starts from an intentionally broken design and ends with the fixed implementation in `src/Portfolio.Api/Program.cs`.

1. **Missing-resource handling**: an absent order must produce a predictable HTTP 404 instead of leaking null-related failures.
2. **Refund business rule**: only `Paid` orders may be refunded. A state transition must be validated before mutation.
3. **Database search performance**: applying `ToLower()` to a database column can prevent effective index use. The fixed design uses a normalized searchable field (or, in SQL Server, a suitable case-insensitive collation/index strategy).
4. **Money precision**: currency uses `decimal` and is mapped to `decimal(18,2)` rather than floating point.
5. **Retry/idempotency**: a repeated refund request must not perform a second refund. Already-refunded orders return their current state.
