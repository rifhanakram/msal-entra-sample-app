# OAuth 2.0 & PKCE Flow - Intern Training Session

## Session Overview
**Duration:** 45-60 minutes  
**Audience:** Interns  
**Goal:** Understanding OAuth 2.0 fundamentals and PKCE implementation

---

## 1. Introduction to OAuth 2.0 (10 minutes)

### What is OAuth?
- **Definition**: Open standard for authorization
- **Purpose**: Allow applications to access user resources without sharing credentials
- **Real-world analogy**: Hotel key card vs master key
- **Not authentication** - that's OpenID Connect (built on OAuth)

### Why OAuth Matters
- Security: No password sharing
- User control: Granular permissions
- Developer experience: Standardized flow
- Scalability: Centralized authorization

### Key Players (The OAuth Dance)
1. **Resource Owner** (User) - "I want to use this app"
2. **Client** (Application) - "I need access to user data"
3. **Authorization Server** (Azure AD/Google) - "I'll verify and grant access"
4. **Resource Server** (API) - "I'll serve data with valid tokens"

---

## 2. OAuth Flow Types (15 minutes)

### Authorization Code Flow (Traditional)
```
User → App → Auth Server → App → API
```
- **Use case**: Web applications with backend
- **Security**: Client secret protects token exchange
- **Problem**: What about mobile/SPA apps without secure storage?

### Implicit Flow (Legacy)
```
User → App → Auth Server → App (directly gets token)
```
- **Use case**: Single Page Applications (SPAs)
- **Problem**: Tokens exposed in browser URL/history
- **Status**: Deprecated due to security concerns

### PKCE to the Rescue! 🛡️
**Proof Key for Code Exchange**
- Solves the "no client secret" problem
- Makes Authorization Code flow secure for public clients

---

## 3. Deep Dive: PKCE Flow (15 minutes)

### The PKCE Problem
**Scenario**: Mobile app needs OAuth but can't store client secrets
- Mobile apps are "public clients"
- Source code can be reverse-engineered
- No secure server-side storage

### PKCE Solution: Dynamic Secrets
Instead of static client secret → generate dynamic proof key

### Step-by-Step PKCE Flow

#### Step 1: Generate Code Challenge
```javascript
// App generates random code verifier
code_verifier = "dBjftJeZ4CVP-mB92K27uhbUJU1p1r_wW1gFWFOEjXk"

// Creates SHA256 hash
code_challenge = SHA256(code_verifier)
code_challenge_method = "S256"
```

#### Step 2: Authorization Request
```
https://login.microsoftonline.com/authorize?
  client_id=your_app_id
  &response_type=code
  &redirect_uri=your_callback
  &scope=openid profile
  &code_challenge=E9Melhoa2OwvFrEMTJguCHaoeK1t8URWbuGJSstw-cM
  &code_challenge_method=S256
```

#### Step 3: User Consent & Authorization Code
- User logs in and consents
- Auth server returns authorization code
- Code is tied to the code_challenge

#### Step 4: Token Exchange
```javascript
POST /token
{
  grant_type: "authorization_code",
  client_id: "your_app_id",
  code: "auth_code_from_step_3",
  code_verifier: "dBjftJeZ4CVP-mB92K27uhbUJU1p1r_wW1gFWFOEjXk"
}
```

#### Step 5: Server Verification
```javascript
// Server verifies:
SHA256(received_code_verifier) === stored_code_challenge
// If match → issue access token
```

---

## 4. PKCE in Practice (10 minutes)

### Where PKCE is Used
- **Mobile Apps**: iOS/Android native apps
- **Single Page Applications**: React, Angular, Vue apps
- **Desktop Apps**: Electron, native desktop applications
- **IoT Devices**: Smart devices without secure storage

### PKCE vs Traditional Flow Comparison

| Aspect | Traditional Flow | PKCE Flow |
|--------|------------------|-----------|
| Client Type | Confidential | Public |
| Secret Storage | Server-side | None needed |
| Security | Client Secret | Dynamic Code Challenge |
| Use Cases | Web apps with backend | Mobile, SPA, Desktop |

### Code Example Walkthrough
*[Demo using the sample app in this repository]*

---

## 5. Security Benefits & Best Practices (10 minutes)

### Security Benefits of PKCE
1. **No Static Secrets**: Can't be extracted from app
2. **One-time Use**: Code verifier is unique per flow
3. **Tamper-proof**: Authorization code tied to specific challenge
4. **Man-in-the-middle Protection**: Intercepted codes are useless

### Best Practices
- **Always use PKCE** for public clients
- **Use S256** (SHA256) challenge method, not "plain"
- **Generate strong code verifier** (43-128 characters, URL-safe)
- **Store code verifier securely** during the flow
- **Validate redirect URIs** strictly
- **Use HTTPS everywhere**

### Common Pitfalls to Avoid
- Using deprecated Implicit flow
- Storing tokens in localStorage (use secure storage)
- Not validating state parameter
- Weak code verifier generation

---

## 6. Hands-on Demo (5 minutes)

### Live Demo Using Our Sample App
1. **Show the network tab** during login
2. **Point out PKCE parameters** in auth request
3. **Demonstrate token exchange**
4. **Show how tokens are used** in API calls

### Key Observations
- Code challenge in authorization URL
- Code verifier in token exchange
- No client secret needed
- Secure token storage

---

## 7. Q&A and Wrap-up (5 minutes)

### Discussion Points
- When would you use OAuth vs other auth methods?
- How does PKCE improve mobile app security?
- What happens if code verifier is compromised?

### Key Takeaways
1. OAuth 2.0 is about **authorization**, not authentication
2. PKCE makes OAuth secure for **public clients**
3. Always use **PKCE with S256** for modern applications
4. **Dynamic secrets** are more secure than static ones

### Further Learning
- [RFC 7636 - PKCE Specification](https://tools.ietf.org/html/rfc7636)
- [OAuth 2.0 Security Best Practices](https://tools.ietf.org/html/draft-ietf-oauth-security-topics)
- Try implementing PKCE in different frameworks
- Explore Azure AD B2C, Auth0, or other providers

---

## Bonus: Quick Quiz Questions
1. What does PKCE stand for?
2. Why can't mobile apps use client secrets securely?
3. What's the difference between code_verifier and code_challenge?
4. When should you use PKCE vs traditional Authorization Code flow?
5. What hash function is recommended for PKCE?

**Answers:**
1. Proof Key for Code Exchange
2. Mobile app code can be reverse-engineered
3. Verifier is the secret, challenge is the SHA256 hash
4. PKCE for public clients (mobile, SPA), traditional for confidential clients
5. SHA256 (S256 method)