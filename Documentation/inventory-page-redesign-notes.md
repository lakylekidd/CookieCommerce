# Inventory Page Redesign — Notes & Resolved Decisions

**Status:** Requirements locked — ready for implementation planning  
**Goal:** Redesign the backoffice Inventory page so all stats are scoped to a selected day, with navigation to previous days. The batches table remains global (all-time). Everything else becomes day-aware.

---

## What Exists Today

### Page layout (Inventory.razor)
```
[ Stat cards row: BakedToday | Remaining | QuotaProgress | Fulfillment ]
[ Left col (4/12): CookieInventoryVsRequired  |  Right col (8/12): BatchesTableCard ]
```

The entire page loads a single `InventoryAreaDto` via `IInventoryService.GetInventoryAsync()`. There is no date parameter — everything is hardcoded as "today."

### The four stat cards
| Card | What it shows | Source field |
|---|---|---|
| **Baked Today** | Cookies baked so far | `OrderQuotaDto.BakedToday` = 12 (hardcoded) |
| **Remaining to Bake** | Cookies still needed | `OrderQuotaDto.RemainingToBake` = 34 (hardcoded) |
| **Daily Quota** | % of capacity used | `OrderQuotaDto.DailyQuotaPercentage` = 32.0 (hardcoded) |
| **Fulfillment** | Orders ready / total orders | `OrderQuotaDto.OrdersReadyToFulfill` / `TotalOrders` = 6/8 |

### Left panel — "Cookies to Bake" (`CookieInventoryVsRequired`)
Shows a list of `InventoryQuotaItemDto`. Each item has:
- A product (name + image)
- `QuantityRequired` — cookies of this type needed to fill today's orders
- `QuantityReadyForFulfillment` — cookies already baked and good
- `Status` — `NotStarted` / `InProgress` / `ReadyForFulfillment`
- A progress bar + status badge

Currently **only 2 products** are hardcoded: Chocolate Chunk (30 required, 0 ready) and Red Velvet (20 required, 12 ready). There are 3 batch types today in the mock data — the third (Matcha) is absent from this panel.

### Right panel — BatchesTableCard
Shows all batches across **all days** (today + 3 days of history) in one paginated table. Sorted descending by `CreatedOn`. Each row expands to show batch runs, baking info, and an inline editor.

---

## Key Business Rules (from Order-Lifecycle.md)

### The quota system
- **Quota = the maximum number of cookies the business can produce per day.** It is a fixed capacity number (currently 50, will come from Settings).
- When a customer places an order, it is assigned to the nearest available quota day that still has capacity. If the current day's quota is full, they get the next day with space.
- **Cutoff time is 19:00.** Orders placed after 19:00 go to the next available quota date, unless that day still has remaining capacity.

### The baker's evening workflow (after 19:00)
1. Baker reviews the inventory page — sees which cookie types and quantities are required for the next day's deliveries.
2. For each required cookie type, baker creates a **Batch** (`BatchStatus.Scheduled`). Each batch has a planned quantity of **12 cookies** (65g each).
3. Baker begins mixing → status changes to `BatchStatus.Mixing`.
4. Mixed dough goes in the fridge overnight → status changes to `BatchStatus.Resting`.

### The baker's morning workflow
1. Baker creates **Batch Runs** for each batch. The oven holds a maximum of **6 cookies at a time**, so a batch of 12 needs at least 2 runs.
2. When a run goes into the oven → `BatchRunStatus.Baking`.
3. When the run comes out and cools → `BatchRunStatus.Cooling`.
4. **The moment a batch run reaches Cooling, its cookies become available.** The baker enters `ActualQuantity` on the parent batch. Available cookies = `ActualQuantity - DefectiveQuantity`.
5. When at least one run of a batch is at Cooling, the **parent batch status = Resting** (even though "resting" previously meant fridge — at this stage it means "partially available").
6. Once all runs are done → `BatchRunStatus.Completed` → parent batch → `BatchStatus.Completed`.
7. Default baking settings: **14 minutes, 175°C**.

### Cookie availability and order allocation
- Cookies become available as soon as a batch run reaches `Cooling`.
- Available count per cookie type = sum of `(ActualQuantity - DefectiveQuantity)` across all batches of that type whose runs have reached Cooling or Completed, for the relevant quota day.
- Orders in `Processing` status are checked against available stock.
- **Allocation is FIFO** — the earliest-placed order gets first claim on available cookies.
- If an order cannot be fully fulfilled, the order line shows which cookie types are missing and how many. The "Ready to Ship" button is disabled until fully covered.

### Critical insight: batches belong to a delivery date, not their creation date
Batches are created in the **evening for the following day's deliveries**. A batch created on Monday at 20:00 contributes to Tuesday's quota. Therefore:
- `BatchDto.CreatedOn` is NOT the right field to scope a batch to a quota day.
- `BatchDto` needs a `DeliveryDate` (DateOnly) field — the quota day the batch's output is intended for.

---

## Questions — All Answered

### Q1 — Daily Quota definition ✅
**Quota % = `BakedToday / CurrentQuota * 100`**  
`CurrentQuota` is the fixed daily oven capacity (currently 50). This answers "how much of our physical capacity have we used today." It is independent of how many orders exist.

### Q2 — "Remaining to Bake" meaning ✅
**Remaining = `TotalCookiesNeeded (from orders) - BakedSoFar`**  
This tells the baker how many order-required cookies are still outstanding, not how much oven capacity is left. It is order-backlog oriented, not capacity-oriented.

### Q3 — Source of "Cookies to Bake" list ✅
**Option (a): orders with delivery date = selected day.**  
The left panel is driven by summing cookie line items across all orders whose quota/delivery date matches the selected day. Each distinct cookie type gets one row.

### Q4 — What makes a cookie "ready for fulfillment"? ✅
**Cookies are available from the moment a batch run reaches `Cooling`** — not just when the whole batch is `Completed`.  
`QuantityReadyForFulfillment` per product = `sum(ActualQuantity - DefectiveQuantity)` across all batches of that product for that quota day where at least one run has reached `Cooling` or `Completed`.

### Q5 — Batches table on previous days ✅
The batches table **always shows all batches, all time** — unchanged by day navigation. Sort remains descending by `CreatedOn`.

### Q6 — Fulfillment card: orders or cookies? ✅
**Order-level.** The card shows `OrdersReadyToFulfill / TotalOrders` for the selected day. An order is "ready" only when all its required cookie types are fully covered by available stock under FIFO allocation.

### Q7 — Viewing past days: data source ✅
**Reconstruct from batch records.** For past days, `BakedOnDay` is derived from `sum(ActualQuantity - DefectiveQuantity)` across completed/resting batches with `DeliveryDate = selectedDate`. No separate snapshot needed.

### Q8 — Day navigation range ✅
**Backwards: as far as mock data exists** (currently 4 days). In production, unlimited.  
**Forwards: disabled beyond today** — you cannot view future quota days on the inventory page (those are anticipated, not produced).

### Q9 — Days with no orders ✅
Show an **empty state** on the Cookies to Bake panel ("No orders scheduled for this day"). Stat cards remain visible but show zeros — so the baker can still see quota capacity even on quiet days.

### Q10 — Batch day ownership ✅
**`CreatedOn` is NOT the right field.** `BatchDto` must gain a `DeliveryDate` (DateOnly) field. This is the quota day the batch was created to serve. All stats (baked count, remaining, fulfillment) are scoped using `DeliveryDate`, not `CreatedOn`.

---

## Corrected Data Model Understanding

### `OrderQuotaDto` — how fields should be computed
| Field | Correct Formula |
|---|---|
| `BakedToday` | `sum(ActualQty - DefectQty)` for batches with `DeliveryDate = selectedDate` where a run is ≥ Cooling |
| `RemainingToBake` | `TotalCookiesNeeded - BakedToday` (order backlog, not capacity) |
| `CurrentQuota` | Fixed capacity from Settings (= 50) |
| `DailyQuotaPercentage` | `BakedToday / CurrentQuota` |
| `TotalOrders` | Count of orders with delivery date = selectedDate |
| `OrdersReadyToFulfill` | Orders fully covered by FIFO stock allocation |

### `BatchDto` — new field needed
```
DeliveryDate: DateOnly   // the quota day this batch's output is for
```

### `InventoryQuotaItemDto` — how it should be built
Derived per cookie type from orders for the selected date:
```
QuantityRequired            = sum of that cookie type across all orders for selectedDate
QuantityReadyForFulfillment = sum(ActualQty - DefectQty) from batches for that type/date
                              where at least one BatchRun is Cooling or Completed
```

---

## Gaps in the Current Code That Must Be Fixed

1. **`BatchDto` has no `DeliveryDate`** — all date-scoping logic is blocked until this exists.
2. **`IInventoryService.GetInventoryAsync()` has no date parameter** — add `GetInventoryForDateAsync(DateOnly date)`.
3. **`MockInventoryData` is all static fields** — must become a date-parameterised generator method.
4. **`InventoryArea.InventoryQuota` is hardcoded** — must be computed from mock orders per date.
5. **`OrderQuotaDto` values are hardcoded** — must be computed from batch data and order data.
6. **No mock orders with delivery dates exist in inventory context** — need a `MockOrdersByDeliveryDate` data set to drive the quota items.
7. **`QuantityReadyForFulfillment` is never updated by batch run progress** — needs to be derived, not stored.

---

## Finalised UX Blueprint

```
┌─────────────────────────────────────────────────────┐
│  ← Prev Day   │  Thursday 5 Jun 2025 · Today  │ → Today (disabled)
└─────────────────────────────────────────────────────┘

[ Baked: 12  ]  [ Remaining: 22 ]  [ Quota: 24% of 50 ]  [ Fulfillment: 6/8 orders ]
  (actual-def    (orders need 34    (12/50 capacity)        (order-level FIFO)
   from batches)  baked 12 so far)

┌─────────────────────────────────┐  ┌────────────────────────────────────────────┐
│ Cookies to Bake · Thu 5 Jun     │  │ All Batches · All Time                     │
│                                 │  │ ─────────────────────────────────────────  │
│ 🍪 Choc Chunk   0/30  ░░ Wait   │  │ REF-225 │ Choc Chunk │ Mixing   │ Today    │
│ 🍪 Red Velvet  12/20  ▓▓ Prog   │  │ REF-226 │ Matcha     │ Scheduled│ Today    │
│ 🍪 Matcha       0/14  ░░ Wait   │  │ REF-227 │ Red Velvet │ Resting  │ Today    │
│                                 │  │ REF-228 │ Hazelnut   │ Completed│ Yesterday│
│                                 │  │ ...     │            │          │          │
└─────────────────────────────────┘  └────────────────────────────────────────────┘
```

**Viewing a past day (e.g. Yesterday):**
- Stat cards update to show that day's baked count, quota %, and fulfillment.
- Cookies to Bake panel shows yesterday's order requirements and how many were produced.
- "Baked Today" label becomes "Baked on Wed 4 Jun".
- Batches table unchanged.
- "Next Day →" button is enabled (unless yesterday IS today — shouldn't happen).
