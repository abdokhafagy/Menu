# Mobile Bottom Navigation Redesign (Production-Ready)

## 🎯 Goal

Build a dynamic, role-aware mobile bottom navigation that:

- Shows **icons only**
- Displays **all allowed items**
- Handles overflow professionally (More menu)
- Adapts to screen size
- Matches modern app UX

---

## 📦 Final Implementation Plan

### 1. Bottom Navigation Component (Clean Implementation)

- **File**: `Menu.UI/Components/BottomNav.razor`

### 🔥 Change:

- DO NOT hide text using CSS hacks
- Create a clean reusable component:

Example:

- `<IconNavItem />` (icon-only rendering)

👉 Keeps UI clean + maintainable

---

### 2. Navigation Model (IMPORTANT 🔥)

- **File**: `Menu.UI/Navigation/AppNavigation.cs`

### 🔥 Update Model:

```csharp
public class NavItem
{
    public string Title { get; set; }
    public string Icon { get; set; }
    public string Route { get; set; }

    public string[]? Roles { get; set; }
    public string[]? Permissions { get; set; }

    public int Priority { get; set; } = 0; // 🔥 NEW
    public bool ShowOnMobile { get; set; } = true; // 🔥 NEW
}
```

---

### 3. Smart Filtering + Sorting

- **File**: `NavigationFilter.cs`

### 🔥 Update Logic:

- Filter by:
  - Roles
  - Permissions
  - ShowOnMobile

- THEN sort:

```csharp
.OrderBy(x => x.Priority)
```

---

### 4. Responsive Max Items (Dynamic 🔥)

Instead of fixed 4 items:

```csharp
int maxItems = screenWidth < 400 ? 3 : 4;
```

---

### 5. Overflow Handling (Improved)

### ✅ Logic:

- If items <= maxItems → show all
- If items > maxItems:
  - Show (maxItems - 1)
  - Add "More" button

---

### 6. "More" Bottom Sheet (Enhanced UX)

- Use:
  - `MudDrawer Anchor="Bottom"`

### 🔥 Improvements:

- Show:
  - Icon + Label

- Highlight active item
- Add smooth animation (slide-up)
- Close on selection

---

### 7. Active Route Handling (IMPORTANT)

- Detect current route
- Highlight:
  - Bottom bar item
  - AND overflow item

---

### 8. Performance Optimization

### 🔥 Add caching:

```csharp
_cachedItems ??= NavigationFilter.Get(context);
```

- Avoid recalculating every render

---

### 9. Sidebar + BottomNav Sync

- Use SAME navigation config
- No duplication

---

### 10. UI/UX Improvements

- Ripple effect on click
- Active state (color/background)
- Consistent spacing
- Touch-friendly icons

---

### 11. Responsive Behavior

- Desktop → Sidebar
- Mobile → BottomNav

(Keep existing MudHidden logic ✔)

---

## 📦 Output

Provide:

1. BottomNav.razor (full implementation)
2. IconNavItem component
3. Updated NavItem model
4. NavigationFilter logic
5. Overflow (More) implementation
6. Active route handling
7. Responsive detection logic

---

## 🎯 Final Goal

Create a **smart, scalable, and high-performance navigation system**:

- Role-aware
- Permission-aware
- Fully responsive
- Clean UX
- Production-ready
