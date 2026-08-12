	# Talent Note Request Lifecycle

This document explains the full request lifecycle for creating a Talent Note in the MetaMind backend, including the “glue” that makes MediatR, validators, handlers, repositories, and EF Core connect to each other.

The feature lives inside the `Talent` bounded context/module because a note is information attached to a talent profile. Even though an admin creates the note, the business concept belongs to the Talent domain.

---

## Feature being explained

Create a note for a talent:

```http
POST /api/TalentNote/notes?talentId={talentId}
```

Example body:

```json
{
  "content": "Student showed strong interest in robotics.",
  "category": "Interview"
}
```

The request creates a `TalentNote` record linked to an existing talent profile.

---

## High-level flow

```text
HTTP Request
→ TalentNoteController
→ IMediator.Send(command)
→ MediatR pipeline behaviors
   → ValidationBehaviour
   → TalentActiveValidationBehaviour
→ CreateTalentNoteHandler
→ TalentNote.Create(...)
→ ITalentNoteRepository
→ TalentNoteRepository
→ TalentDbContext
→ PostgreSQL table: talent.TalentNotes
→ Response returned to controller
→ HTTP 201 Created
```

---

## 1. API entry point: Controller

File:

```text
src/Modules/Talent/MMM.Talent.API/Controllers/TalentNoteController.cs
```

The controller exposes the endpoint:

```csharp
[HttpPost("notes")]
public async Task<IActionResult> Create(
    [FromQuery] Guid talentId,
    [FromBody] CreateTalentNoteCommand command,
    CancellationToken cancellationToken)
```

The route is based on:

```csharp
[Route("api/[controller]")]
```

Because the controller is named `TalentNoteController`, the route becomes:

```text
api/TalentNote
```

Then `[HttpPost("notes")]` adds:

```text
/notes
```

So the final route is:

```text
POST /api/TalentNote/notes
```

The `talentId` is passed as a query parameter:

```text
POST /api/TalentNote/notes?talentId={talentId}
```

The controller also has:

```csharp
[Authorize(Policy = "AdminPolicy")]
```

So this endpoint is admin-only.

---

## 2. Controller prepares the command

The request body is bound into:

```text
CreateTalentNoteCommand
```

File:

```text
src/Modules/Talent/MMM.Talent.Application/Features/TalentNotes/CreateTalentNote/CreateTalentNoteCommand.cs
```

The command shape is:

```csharp
public record CreateTalentNoteCommand : IRequest<TalentNoteResponse>, ITalentScopedCommand
{
    public Guid TalentId { get; init; }
    public string Content { get; init; } = string.Empty;
    public string? Category { get; init; }
    public Guid CreatedById { get; init; }
}
```

The client sends only:

```json
{
  "content": "...",
  "category": "..."
}
```

The controller fills the trusted server-side values:

```csharp
command with
{
    TalentId = talentId,
    CreatedById = currentUser.UserId
}
```

This is important because the API does not trust the client to decide who created the note. `CreatedById` comes from the authenticated current user.

---

## 3. Controller sends the command through MediatR

Still in:

```text
TalentNoteController.cs
```

The controller calls:

```csharp
var result = await mediator.Send(
    command with
    {
        TalentId = talentId,
        CreatedById = currentUser.UserId
    },
    cancellationToken);
```

The controller does not directly create the note.

It does not directly call the handler.

It does not directly call the repository.

Instead, it sends the command to MediatR.

At this point, MediatR becomes the dispatcher.

---

## 4. The MediatR “glue”: how MediatR knows about handlers

MediatR knows about handlers because the Talent Application assembly is registered during application startup.

File:

```text
src/Modules/Talent/MMM.Talent.Application/Extensions/TalentApplicationExtensions.cs
```

Relevant code:

```csharp
services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(CreateTalentCommand).Assembly));
```

This tells MediatR:

```text
Scan the MMM.Talent.Application assembly.
Find all IRequestHandler<,> implementations.
Register them in dependency injection.
```

Because `CreateTalentNoteHandler` is in the same application assembly, MediatR discovers it.

File:

```text
src/Modules/Talent/MMM.Talent.Application/Features/TalentNotes/CreateTalentNote/CreateTalentNoteHandler.cs
```

The handler declares:

```csharp
internal sealed class CreateTalentNoteHandler(ITalentNoteRepository repository)
    : IRequestHandler<CreateTalentNoteCommand, TalentNoteResponse>
```

That type signature is the key.

It says:

```text
I handle CreateTalentNoteCommand.
I return TalentNoteResponse.
```

The command also says:

```csharp
public record CreateTalentNoteCommand : IRequest<TalentNoteResponse>
```

So MediatR connects them like this:

```text
CreateTalentNoteCommand : IRequest<TalentNoteResponse>
CreateTalentNoteHandler : IRequestHandler<CreateTalentNoteCommand, TalentNoteResponse>
```

That matching pair is how MediatR knows which handler to call.

---

## 5. The validation “glue”: how validators run before handlers

The validator is:

```text
src/Modules/Talent/MMM.Talent.Application/Features/TalentNotes/CreateTalentNote/CreateTalentNoteValidator.cs
```

It declares:

```csharp
public sealed class CreateTalentNoteValidator
    : AbstractValidator<CreateTalentNoteCommand>
```

That type signature means:

```text
I validate CreateTalentNoteCommand.
```

The rules are:

```csharp
RuleFor(x => x.TalentId)
    .NotEmpty()
    .WithMessage("TalentId is required.");

RuleFor(x => x.CreatedById)
    .NotEmpty()
    .WithMessage("CreatedById is required.");

RuleFor(x => x.Content)
    .NotEmpty()
    .MaximumLength(1000);

RuleFor(x => x.Category)
    .MaximumLength(100)
    .When(x => x.Category is not null);
```

But MediatR does not automatically know about FluentValidation by itself.

Two registration lines create the glue.

File:

```text
src/Modules/Talent/MMM.Talent.Application/Extensions/TalentApplicationExtensions.cs
```

First:

```csharp
services.AddValidatorsFromAssembly(typeof(CreateTalentCommand).Assembly);
```

This tells FluentValidation:

```text
Scan the Talent Application assembly.
Find all AbstractValidator<T> classes.
Register them in dependency injection.
```

So `CreateTalentNoteValidator` gets registered as:

```text
IValidator<CreateTalentNoteCommand>
```

Second:

```csharp
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
```

This tells MediatR:

```text
Before executing a handler, run ValidationBehaviour.
```

The behavior is a pipeline step. It wraps the handler.

Conceptually:

```text
mediator.Send(command)
→ ValidationBehaviour.Handle(...)
→ actual handler
```

Inside the validation behavior, the project resolves validators for the current request type.

For `CreateTalentNoteCommand`, dependency injection returns:

```text
CreateTalentNoteValidator
```

So the command is validated before `CreateTalentNoteHandler` is allowed to run.

If validation fails, the handler is never called.

---

## 6. The talent-active guard “glue”

The create command also implements:

```csharp
ITalentScopedCommand
```

File:

```text
src/Modules/Talent/MMM.Talent.Application/Abstractions/ITalentScopedCommand.cs
```

That interface is:

```csharp
public interface ITalentScopedCommand
{
    Guid TalentId { get; }
}
```

This tells the application:

```text
This command operates inside a specific talent's scope.
```

There is another MediatR pipeline behavior:

```text
src/Modules/Talent/MMM.Talent.Application/Behaviours/TalentActiveValidationBehaviour.cs
```

It checks:

```csharp
if (request is ITalentScopedCommand scoped &&
    !await talentRepository.IsActiveAsync(scoped.TalentId, cancellationToken))
{
    throw new NotFoundException(
        $"Talent with id '{scoped.TalentId}' was not found or has been deleted.");
}
```

This means:

```text
If a command has a TalentId,
check that the talent exists and is not deleted
before allowing the handler to run.
```

This behavior is also registered in:

```text
TalentApplicationExtensions.cs
```

With:

```csharp
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TalentActiveValidationBehaviour<,>));
```

So the full pre-handler flow is:

```text
mediator.Send(CreateTalentNoteCommand)
→ ValidationBehaviour
→ TalentActiveValidationBehaviour
→ CreateTalentNoteHandler
```

---

## 7. Handler creates the domain entity

File:

```text
src/Modules/Talent/MMM.Talent.Application/Features/TalentNotes/CreateTalentNote/CreateTalentNoteHandler.cs
```

The handler receives the validated command:

```csharp
public async Task<TalentNoteResponse> Handle(
    CreateTalentNoteCommand command,
    CancellationToken cancellationToken)
```

It creates a domain entity:

```csharp
var note = TalentNote.Create(
    talentId: command.TalentId,
    content: command.Content.Trim(),
    createdById: command.CreatedById,
    category: string.IsNullOrWhiteSpace(command.Category) ? null : command.Category.Trim());
```

The domain entity is:

```text
src/Modules/Talent/MMM.Talent.Domain/TalentNote.cs
```

It has:

```csharp
public class TalentNote : BaseEntity
{
    private TalentNote() { }

    public static TalentNote Create(
        Guid talentId,
        string content,
        Guid createdById,
        string? category = null)
    {
        return new TalentNote
        {
            TalentId = talentId,
            Content = content,
            Category = category,
            CreatedById = createdById
        };
    }
}
```

The private constructor exists for EF Core materialization.

The static `Create` method is the domain-friendly creation point.

---

## 8. Handler saves through a repository interface

The handler does not depend directly on EF Core.

It depends on:

```text
ITalentNoteRepository
```

File:

```text
src/Modules/Talent/MMM.Talent.Application/Abstractions/ITalentNoteRepository.cs
```

Interface:

```csharp
public interface ITalentNoteRepository
{
    Task AddAsync(TalentNote note, CancellationToken cancellationToken = default);
    Task<List<TalentNote>> GetByTalentIdAsync(Guid talentId, CancellationToken cancellationToken = default);
}
```

The handler calls:

```csharp
await repository.AddAsync(note, cancellationToken);
```

This keeps the Application layer independent of EF Core.

DDD/Clean Architecture idea:

```text
Application layer knows the repository abstraction.
Infrastructure layer provides the repository implementation.
```

---

## 9. The repository DI “glue”: how the interface becomes the real repository

The handler asks for:

```text
ITalentNoteRepository
```

But the actual class is:

```text
TalentNoteRepository
```

The connection is registered here:

```text
src/Modules/Talent/MMM.Talent.Infrastructure/Extensions/TalentInfrastructureExtensions.cs
```

Relevant line:

```csharp
services.AddScoped<ITalentNoteRepository, TalentNoteRepository>();
```

This tells dependency injection:

```text
Whenever something asks for ITalentNoteRepository,
create/provide a TalentNoteRepository.
```

So when MediatR creates `CreateTalentNoteHandler`, DI sees this constructor:

```csharp
CreateTalentNoteHandler(ITalentNoteRepository repository)
```

And injects:

```text
TalentNoteRepository
```

That is the glue between Application and Infrastructure.

---

## 10. Infrastructure repository writes to EF Core

File:

```text
src/Modules/Talent/MMM.Talent.Infrastructure/Repositories/TalentNoteRepository.cs
```

The repository implementation:

```csharp
public sealed class TalentNoteRepository(TalentDbContext context) : ITalentNoteRepository
{
    public async Task AddAsync(TalentNote note, CancellationToken cancellationToken = default)
    {
        await context.TalentNotes.AddAsync(note, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }
}
```

This is where the entity is actually persisted.

The repository uses:

```text
TalentDbContext
```

which is the EF Core database context for the Talent module.

---

## 11. DbContext exposes the TalentNotes table

File:

```text
src/Modules/Talent/MMM.Talent.Infrastructure/Data/TalentDbContext.cs
```

The new DbSet:

```csharp
public DbSet<TalentNote> TalentNotes => Set<TalentNote>();
```

This lets EF Core understand:

```text
TalentNote entity
→ TalentNotes database table
```

The mapping config is:

```csharp
modelBuilder.Entity<TalentNote>(entity =>
{
    entity.HasKey(e => e.Id);
    entity.HasIndex(e => e.TalentId);
    entity.Property(e => e.Content).IsRequired().HasMaxLength(1000);
    entity.Property(e => e.Category).HasMaxLength(100);
    entity.Property(e => e.CreatedById).IsRequired();
    entity.Property(e => e.Extra).HasColumnType("jsonb");

    entity.HasOne(e => e.Profile)
          .WithMany(p => p.TalentNotes)
          .HasForeignKey(e => e.TalentId)
          .HasPrincipalKey(p => p.TalentId)
          .OnDelete(DeleteBehavior.Cascade);

    entity.HasQueryFilter(e => !e.IsDeleted);
    entity.Property(e => e.IsDeleted).HasDefaultValue(false);
});
```

Important pieces:

```csharp
entity.HasOne(e => e.Profile)
      .WithMany(p => p.TalentNotes)
```

This links:

```text
One Profile
→ many TalentNotes
```

The foreign key is:

```csharp
.HasForeignKey(e => e.TalentId)
```

The principal key is:

```csharp
.HasPrincipalKey(p => p.TalentId)
```

So notes connect to `Profile.TalentId`, not `Profile.Id`.

That matches the existing Talent module pattern used by entities like `Achievement`.

---

## 12. Profile navigation property

File:

```text
src/Modules/Talent/MMM.Talent.Domain/Profile.cs
```

The profile now has:

```csharp
public ICollection<TalentNote> TalentNotes { get; set; } = new List<TalentNote>();
```

This lets EF Core understand the relationship from the profile side:

```text
Profile
→ TalentNotes
```

---

## 13. Migration creates the database table

File:

```text
src/Modules/Talent/MMM.Talent.Infrastructure/Migrations/20260812074100_AddTalentNotes.cs
```

The migration creates the database structure for the new entity.

Conceptually, it creates:

```text
schema: talent
table: TalentNotes
```

With fields like:

```text
Id
CreatedAt
UpdatedAt
IsDeleted
Extra
TalentId
Content
Category
CreatedById
```

This is what makes the EF model real in the database.

---

## 14. Handler returns a response

Back in:

```text
CreateTalentNoteHandler.cs
```

After saving, the handler returns:

```csharp
return new TalentNoteResponse(
    note.Id,
    note.TalentId,
    note.Content,
    note.Category,
    note.CreatedById,
    note.CreatedAt,
    note.UpdatedAt);
```

The response type is:

```text
src/Modules/Talent/MMM.Talent.Application/Features/TalentNotes/TalentNoteResponse.cs
```

```csharp
public sealed record TalentNoteResponse(
    Guid Id,
    Guid TalentId,
    string Content,
    string? Category,
    Guid CreatedById,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
```

This goes back through MediatR to the controller.

Then the controller returns:

```csharp
return Created($"api/talentnote/notes/{result.Id}", result);
```

So the client receives:

```http
201 Created
```

With a response body like:

```json
{
  "id": "note-guid",
  "talentId": "talent-guid",
  "content": "Student showed strong interest in robotics.",
  "category": "Interview",
  "createdById": "admin-guid",
  "createdAt": "2026-08-12T07:41:00Z",
  "updatedAt": null
}
```

---

## Full create lifecycle, with glue included

```text
1. Program startup
   → AddTalentApplication()
      → registers MediatR handlers
      → registers FluentValidation validators
      → registers pipeline behaviors

   → AddTalentInfrastructure()
      → maps ITalentNoteRepository to TalentNoteRepository

2. Runtime request
   → POST /api/TalentNote/notes?talentId={talentId}

3. ASP.NET Core routing
   → finds TalentNoteController.Create(...)

4. Model binding
   → query string talentId becomes Guid talentId
   → JSON body becomes CreateTalentNoteCommand

5. Controller
   → fills TalentId from query
   → fills CreatedById from current authenticated user
   → calls mediator.Send(command)

6. MediatR
   → sees CreateTalentNoteCommand implements IRequest<TalentNoteResponse>
   → finds IRequestHandler<CreateTalentNoteCommand, TalentNoteResponse>

7. Pipeline behavior: ValidationBehaviour
   → asks DI for IValidator<CreateTalentNoteCommand>
   → receives CreateTalentNoteValidator
   → validates Content, TalentId, CreatedById, Category

8. Pipeline behavior: TalentActiveValidationBehaviour
   → sees command implements ITalentScopedCommand
   → checks TalentId exists and is active

9. Handler
   → CreateTalentNoteHandler.Handle(...)
   → creates TalentNote domain entity

10. Repository abstraction
   → handler calls ITalentNoteRepository.AddAsync(note)

11. Dependency injection
   → ITalentNoteRepository resolves to TalentNoteRepository

12. Infrastructure repository
   → calls context.TalentNotes.AddAsync(note)
   → calls context.SaveChangesAsync()

13. EF Core
   → uses TalentDbContext mapping
   → writes to talent.TalentNotes

14. Response
   → handler returns TalentNoteResponse
   → MediatR returns it to controller
   → controller returns 201 Created
```

---

## The shortest mental model

```text
Controller = receives HTTP
Command = describes what should happen
Validator = checks request shape/rules
Pipeline behavior = runs shared logic before handler
Handler = performs the use case
Domain entity = business object being created
Repository interface = application contract for persistence
Repository implementation = EF Core persistence code
DbContext = database mapping/session
Migration = actual database schema change
Extensions = startup glue/registration
```

The most important thing to remember:

```text
Extensions are not in the request path directly.
They wire everything together before requests happen.
```

At runtime, the actual request path is:

```text
Controller
→ MediatR
→ Pipeline Behaviors
→ Handler
→ Repository
→ DbContext
→ Database
```
