# Custom Hooks - Developer Guide

## Overview

These custom hooks encapsulate common patterns used across the Fake Survey Generator frontend, reducing code duplication and improving component clarity.

## Hooks Available

### `useApiCall()`

**Purpose**: Centralized authenticated API call handler with Auth0 integration

**Signature**:
```typescript
function useApiCall(): UseApiCallResult

interface UseApiCallResult {
  apiCall: (url: string, options?: FetchOptions) => Promise<Response>;
}
```

**Features**:
- Automatic Auth0 token acquisition
- Standard JSON content-type headers
- Bearer token authentication
- Custom headers support
- Memoized for performance

**Example**:
```tsx
import { useApiCall } from "../hooks";

function MyComponent() {
  const { apiCall } = useApiCall();

  const handleFetch = async () => {
    try {
      const response = await apiCall("api/survey", {
        method: "POST",
        body: JSON.stringify(surveyData),
      });
      
      if (!response.ok) throw new Error("Failed");
      const data = await response.json();
      return data;
    } catch (error) {
      console.error("API call failed:", error);
    }
  };

  return <button onClick={handleFetch}>Fetch Data</button>;
}
```

**Common Use Cases**:
- Making authenticated POST requests
- Fetching data with automatic token injection
- Any API call that requires Auth0 authentication

---

### `useSurveyFetch(surveyId: number | null)`

**Purpose**: Fetch and manage a single survey with automatic state handling

**Signature**:
```typescript
function useSurveyFetch(
  surveyId: number | null
): UseSurveyFetchResult

interface UseSurveyFetchResult {
  survey: SurveyModel | null;
  loading: boolean;
  error: string;
}
```

**Features**:
- Automatic fetch on surveyId change
- Loading state management
- Error state with descriptive messages
- Handles 404 responses
- Automatically cleans up and cancels requests
- Returns null for empty/zero IDs

**Example**:
```tsx
import { useSurveyFetch } from "../hooks";

function SurveyViewer({ surveyId }: { surveyId: number }) {
  const { survey, loading, error } = useSurveyFetch(surveyId);

  if (loading) return <div>Loading survey...</div>;
  if (error) return <div className="error">{error}</div>;
  if (!survey) return <div>No survey found</div>;

  return <SurveyDisplay survey={survey} />;
}
```

**Common Use Cases**:
- Fetching a survey to display
- Auto-refetch when survey ID changes
- Combining with other hooks for complex flows

---

## Migration Guide

### Replacing Direct `fetch()` Calls

**Before**:
```tsx
const [data, setData] = useState(null);
const [loading, setLoading] = useState(false);
const { getAccessTokenSilently } = useAuth0();

useEffect(() => {
  const fetch = async () => {
    setLoading(true);
    const token = await getAccessTokenSilently();
    const response = await fetch("api/survey", {
      headers: { Authorization: `Bearer ${token}` },
    });
    const json = await response.json();
    setData(json);
    setLoading(false);
  };
  fetchData();
}, [getAccessTokenSilently]);
```

**After**:
```tsx
const { survey, loading } = useSurveyFetch(surveyId);
// That's it! State handling is built-in
```

### Replacing Scattered State

**Before**:
```tsx
const [field1, setField1] = useState("");
const [field2, setField2] = useState("");
const [field3, setField3] = useState(0);
const [error, setError] = useState("");
const [success, setSuccess] = useState("");
```

**After**:
```tsx
interface FormState {
  fields: { field1: string; field2: string; field3: number };
  messages: { error: string; success: string };
}

const [formState, setFormState] = useState<FormState>(initialState);
```

---

## Best Practices

### ✅ DO

- Use hooks at the top level of components
- Memoize callback dependencies
- Check for null/empty values before rendering
- Handle both loading and error states
- Use TypeScript for type safety

### ❌ DON'T

- Call hooks conditionally
- Use hooks in loops
- Ignore error states
- Over-complicate hook logic
- Use hooks in nested/anonymous functions

---

## Adding New Hooks

### Template

```typescript
import { useCallback } from "react";

interface UseCustomHookResult {
  // Return type properties
}

/**
 * Brief description of what the hook does
 */
export function useCustomHook(): UseCustomHookResult {
  // Implementation

  return {
    // Return values
  };
}
```

### Checklist

- [ ] Add JSDoc comment with description
- [ ] Include proper TypeScript types
- [ ] Memoize callbacks with `useCallback`
- [ ] Handle edge cases (null, undefined, errors)
- [ ] Export from `src/hooks/index.ts`
- [ ] Add to this documentation
- [ ] Write example usage

---

## Troubleshooting

### Hook is not updating on dependency change

**Issue**: Hook memo is not clearing
**Solution**: Check if dependency array is correct in `useEffect`

### TypeScript errors in hooks

**Issue**: Type mismatch with hook return values
**Solution**: Ensure component destructures all properties correctly

### Auth0 token not being sent

**Issue**: API calls failing with 401 Unauthorized
**Solution**: Verify `useAuth0()` is available in component tree with provider

---

## Performance Tips

1. **Memoize callbacks**: Use `useCallback` when passing callbacks to hooks
2. **Avoid prop drilling**: Use hooks directly in consuming components
3. **Lazy load data**: Only fetch when necessary
4. **Combine related state**: Use object state instead of multiple `useState`

---

## Related Documentation

- [React Hooks Documentation](https://react.dev/reference/react/hooks)
- [Auth0 React SDK](https://auth0.com/docs/libraries/auth0-react)
- [Fake Survey Generator API Docs](../README.md)
