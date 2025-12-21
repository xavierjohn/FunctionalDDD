# ?? Quick Reference: Demo Presenter Guide

**Print this or keep it on a second screen during your presentation!**

---

## ?? **5-Minute Lightning Demo** (Complete Scaffold)

### **Preparation** (Before audience arrives)
```powershell
# Have these variables ready
$REPO_NAME = "demo-ecommerce"
$SPEC_FILE = ".github/project-spec-demo.yml"
```

### **Live Demo**
```powershell
# 1. CREATE REPO (30 sec)
gh repo create $REPO_NAME --public
cd $REPO_NAME

# 2. ADD SPEC (copy pre-written file) (30 sec)
# Show: "This YAML defines our entire application"
Get-Content $SPEC_FILE  # Show it briefly

# 3. TRIGGER (30 sec)
git add .
git commit -m "Add specification"
git push
gh issue create --label "copilot-scaffold" --title "Scaffold E-Commerce System"

# 4. WATCH (2 min - talk while it runs)
# Show: Issue ? Workflow running ? PR created

# 5. REVIEW PR (1 min)
gh pr view --web
# Browse: Domain/Order.cs, Application/Queries, Api/Controllers

# 6. MERGE & TEST (30 sec)
gh pr merge --squash
git pull
dotnet test  # All green!
cd Api/src
dotnet run

# 7. SHOW SWAGGER (1 min)
# Open: https://localhost:5001
# Demo: POST /orders with test data
```

**Key Talking Points**:
- "100 lines of YAML ? 5,000+ lines of production code"
- "Notice: Railway-Oriented Programming in every controller"
- "Tests auto-generated and passing"
- "Swagger docs ready immediately"

---

## ?? **10-Minute Full Demo** (Iterative)

### **Phase 1: Scaffold** (2 min)
```powershell
# Use minimal spec
gh repo create $REPO_NAME --public
cd $REPO_NAME

# Copy .github/project-spec-minimal.yml
Copy-Item "path\to\project-spec-minimal.yml" ".github\project-spec.yml"

git add .
git commit -m "Initial spec"
git push

gh issue create --label "copilot-scaffold" --title "Create project"
# Merge PR when ready
```

**Say**: "We start with just the basics - clean architecture structure"

---

### **Phase 2: Add Aggregate** (2 min)
```powershell
$body = @"
## Feature Request
**Type**: aggregate
**Layer**: domain

## Specification
``````yaml
feature:
  type: aggregate
  layer: domain
aggregate:
  name: Order
  id: OrderId
  # ... rest of spec
``````
"@

gh issue create `
  --label "copilot-feature" `
  --label "domain" `
  --title "Add Order aggregate" `
  --body $body
```

**Say**: "Now let's add our core business entity - the Order"

**Show in PR**:
- Order.cs with Submit() behavior
- OrderId value object
- Money value object  
- 10+ tests

---

### **Phase 3: Add Command** (2 min)
```powershell
$body = @"
``````yaml
feature:
  type: command
  layer: application
command:
  name: CreateOrder
  # ... rest of spec
``````
"@

gh issue create `
  --label "copilot-feature" `
  --label "application" `
  --title "Add CreateOrder command" `
  --body $body
```

**Say**: "Application layer - CQRS pattern in action"

**Show in PR**:
- CreateOrderCommand
- CreateOrderCommandHandler
- IOrderRepository interface

---

### **Phase 4: Add Repository** (1 min)
```powershell
$body = @"
``````yaml
feature:
  type: repository
  layer: acl
repository:
  name: OrderRepository
  # ... rest of spec
``````
"@

gh issue create `
  --label "copilot-feature" `
  --label "acl" `
  --title "Add OrderRepository" `
  --body $body
```

**Say**: "ACL layer - infrastructure implementation"

---

### **Phase 5: Add Endpoint** (2 min)
```powershell
$body = @"
``````yaml
feature:
  type: endpoint
  layer: api
endpoint:
  controller: Orders
  # ... rest of spec
``````
"@

gh issue create `
  --label "copilot-feature" `
  --label "api" `
  --title "Add POST /orders endpoint" `
  --body $body
```

**Say**: "Finally, expose as API with Railway-Oriented Programming"

**Show in PR**:
- OrdersController
- Railway-oriented POST method
- Swagger update

---

### **Phase 6: Demo It!** (1 min)
```powershell
dotnet test  # All green
cd Api/src
dotnet run
# Open Swagger, test POST /orders
```

**Say**: "And it works! From zero to working API in 10 minutes"

---

## ?? **Key Messages to Repeat**

1. **"Each feature is a separate PR"**
   - Reviewable, rollback-able, educational

2. **"Tests are auto-generated"**
   - No manual test writing
   - Covers success and failure paths

3. **"Railway-Oriented Programming everywhere"**
   - No exceptions, explicit errors
   - Clean, readable code

4. **"Production-ready from day one"**
   - Best practices enforced
   - Documentation included
   - Observability built-in

5. **"Works for real projects, not just demos"**
   - Start minimal, add features iteratively
   - Team collaboration through issues
   - Learn as you go

---

## ?? **Visual Aids**

### **Show These in Order**:

1. **YAML Specification** - "This is all you write"
2. **GitHub Issue** - "Just create an issue"
3. **Workflow Running** - "Copilot is working"
4. **PR Created** - "Code ready for review"
5. **Generated Code** - "Notice the patterns"
6. **Tests Passing** - "All green"
7. **Swagger UI** - "Working API immediately"

---

## ?? **Common Demo Pitfalls**

### **Problem**: Network delay
**Fix**: Have backup slides ready, keep talking
**Say**: "While we wait, let me explain what's happening..."

### **Problem**: Workflow fails
**Fix**: Have pre-recorded video as backup
**Say**: "Let me show you what this looks like..." (play video)

### **Problem**: Audience asks about cost
**Answer**: "GitHub Copilot subscription is $10-20/month. Time saved pays for itself in first hour."

### **Problem**: "Can it do X?"
**Answer**: "Show me the domain model and I'll add it as a feature!" (Take their example, create issue live)

---

## ?? **Pre-Demo Checklist**

**30 Minutes Before**:
- [ ] Test entire demo flow once
- [ ] Clear PowerShell history: `Clear-History`
- [ ] Increase terminal font size
- [ ] Set up two screens (code + Swagger)
- [ ] Have backup plan ready
- [ ] Close unnecessary applications
- [ ] Silence notifications

**10 Minutes Before**:
- [ ] Open browser to GitHub
- [ ] Authenticate GitHub CLI: `gh auth status`
- [ ] Have YAML specs ready to copy
- [ ] Test internet connection
- [ ] Start recording (backup)

**Just Before**:
- [ ] Take a breath!
- [ ] Smile
- [ ] Enthusiastic energy!

---

## ?? **Audience-Specific Tips**

### **For Developers**:
- Show code details
- Explain patterns
- Emphasize best practices
- Offer to add features live

### **For Managers/Executives**:
- Focus on time savings
- Show ROI (40 hours ? 10 minutes)
- Emphasize production-ready
- Talk about team productivity

### **For Architects**:
- Emphasize clean architecture
- Discuss separation of concerns
- Highlight extensibility
- Show how to customize patterns

### **For Students/Learners**:
- Focus on iterative approach
- Explain each pattern
- Show how to learn from PRs
- Encourage experimentation

---

## ?? **Q&A: Quick Answers**

**Q**: "Does this replace developers?"
**A**: "No - it replaces tedious scaffolding. Developers focus on business logic and architecture decisions."

**Q**: "What about security?"
**A**: "All code is reviewed in PRs before merge. Plus, security best practices are built into patterns."

**Q**: "Can we customize the patterns?"
**A**: "Yes - edit `.github/copilot-instructions.md` to change conventions."

**Q**: "Works with existing projects?"
**A**: "Absolutely - add features to any project with the agent configured."

**Q**: "How do you handle breaking changes?"
**A**: "Each feature is a separate PR - review before merge, test thoroughly."

---

## ?? **Opening Lines** (Choose One)

**Option 1 (Bold)**:
> "I'm going to build a production-ready e-commerce system in the next 10 minutes. Not a toy app - a real system with domain logic, CQRS, tests, and API documentation. Watch this."

**Option 2 (Relatable)**:
> "How many of you have spent days setting up a new project? Getting the layers right, adding validation, writing tests? Today I'll show you how to do all that in minutes."

**Option 3 (Provocative)**:
> "What if I told you that 90% of the code you write is boilerplate that could be generated automatically? Let me prove it."

---

## ?? **Closing Lines** (Choose One)

**Option 1 (Summary)**:
> "From zero to production-ready API in 10 minutes. Tests passing, documentation complete, best practices enforced. That's the power of FunctionalDDD with GitHub Copilot."

**Option 2 (Call to Action)**:
> "Try it yourself - all the templates are on GitHub. Start with the minimal scaffold, add features as you need them. Build better software, faster."

**Option 3 (Future Vision)**:
> "This is just the beginning. Imagine your team building features this fast, with this quality, every day. That's the future of software development."

---

## ?? **Time Management**

If running **ahead**:
- Show more PR details
- Run additional tests
- Add another feature
- Take more questions

If running **behind**:
- Skip one feature
- Show pre-merged PRs instead of creating new
- Use backup video
- Focus on final demo

---

## ?? **Success Metrics**

**You'll know it went well if**:
- Audience says "wow" at least once
- Questions show understanding
- People ask for repository link
- At least one person tries it immediately
- You get invited to present again!

---

## ?? **Windows-Specific Quick Commands**

```powershell
# Quick setup
winget install GitHub.cli
winget install Microsoft.WindowsTerminal

# Authentication
gh auth login

# Create here-string for multi-line body
$body = @"
Multi-line
content
here
"@

# Useful aliases (add to $PROFILE)
function gci { gh issue create @args }
function gpc { gh pr create @args }
function gpv { gh pr view --web }

# Test connection
gh auth status
dotnet --version

# Clear terminal
Clear-Host  # or cls
```

---

**Remember**: You're not just showing a tool - you're showing the future of software development. Be confident, be enthusiastic, have fun! ??

**Good luck!** ???
