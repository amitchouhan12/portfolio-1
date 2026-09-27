# Case Study: ASP.NET Core Order API Reliability Fixes

## Problem
An existing order API had five production-risk patterns: inconsistent 404 behavior, unsafe refund transitions, inefficient string filtering, unsafe currency representation, and duplicate side effects on retries.

## Fixes
- Added explicit 404 responses for missing orders.
- Enforced `Paid -> Refunded` as the only valid refund transition.
- Made refund requests idempotent so network retries are safe.
- Kept currency as `decimal` and configured fixed precision.
- Replaced column-side `ToLower()` searching with a normalized search field. In a SQL Server deployment, this should be backed by an index and an appropriate collation strategy.

## Why this is portfolio-worthy
This demonstrates the type of work clients buy as support/debugging tasks: reproduce a failure, identify the root cause, make a focused change, and explain the production impact.

## Important limitation
The portfolio uses EF Core InMemory so it runs without a database server. The SQL-performance point is documented as a SQL Server production pattern and should be demonstrated against SQL Server in the next iteration.
