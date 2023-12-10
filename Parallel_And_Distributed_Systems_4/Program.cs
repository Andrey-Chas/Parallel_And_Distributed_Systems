using Microsoft.AspNetCore.Mvc;
using System.Linq.Expressions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapPost("workers/create", async () =>
{
    return Results.Ok();
});

app.MapDelete("workers/delete", async() =>
{
    return Results.Ok();
});

app.MapGet("workers/all", async() =>
{
    return Results.Ok();
});

app.MapPost("jobs/queue", async () =>
{
    return Results.Ok();
});

app.MapGet("jobs/pending", async () =>
{
    return Results.Ok();
});

app.MapGet("jobs/completed", async () =>
{
    return Results.Ok();
});

app.MapDelete("jobs/delete", async () =>
{
    return Results.Ok();
});

app.Run();

internal class WorkerManager
{
    private IList<Job> _jobs = new List<Job>();
    private IDictionary<Worker, CancellationTokenSource> _workers
        = new Dictionary<Worker, CancellationTokenSource>();
    public IReadOnlySet<Worker>? Workers => _workers as IReadOnlySet<Worker>;
    public void AddWorker()
    {
        var worker = new Worker(_jobs);
        var ctxSource = new CancellationTokenSource();

        _workers.Add(worker, ctxSource);
        worker.Start(ctxSource.Token);
    }

    public void RemoveWorker(string id)
    {
        var worker = _workers
            .Keys
            .Where(e => e.Id == id)
            .FirstOrDefault();

        if (worker is not null)
        {
            var ctx = _workers[worker];
            ctx.Cancel();

            _workers.Remove(worker);
        }
    }

    public Job? RemoveJob(string jobId)
    {
        lock(Worker.Locker)
        {
            var job = _jobs.Where(e => e.Id == jobId).Where(e => e.Status == JobStatus.Pending).SingleOrDefault();

            if (job is not null)
            {
                _jobs.Remove(job);
            }

            return job;
        }
    }

    public IEnumerable<Job> GetJobs(Expression<Func<Job, bool>> filter)
    {
        return _jobs
            .AsQueryable()
            .Where(filter)
            .ToList();
    }
}

internal class Worker
{
    public readonly static object Locker = new object();
    private readonly IEnumerable<Job> _jobs;
    public string Id { get; }

    public Worker(
        IEnumerable<Job> jobs)
    {
        Id = Guid.NewGuid().ToString();
        _jobs = jobs;
    }

    public Task Start(CancellationToken cancellationToken)
    {
        return Task.Run(async () =>
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                Job? jobToExecute = null;
                foreach (var job in _jobs)
                {
                    lock (Locker)
                    {
                        if (job.Status == JobStatus.Pending)
                        {
                            jobToExecute = job;
                            jobToExecute.Take();
                        }
                    }
                }
                if (jobToExecute != null)
                {
                    await ExecuteJob(jobToExecute);
                }

                await Task.Delay(3000);
            }
        }, cancellationToken);
    }
    private Task<Job> ExecuteJob(Job job)
    {
        return Task.Run(async () =>
        {
            Console.WriteLine($"Executing job [{job.Name}]...");

            await job.RunningAsync();

            Console.WriteLine($"Job [{job.Name}] completed for {job.ExecutionTimeInSeconds} seconds...");

            return job.Complete();
        });
    }
}

internal enum JobStatus
{
    Pending,
    Taken,
    Running,
    Completed
}

internal class Job
{
    public Job(string name, int executionTimeInSeconds)
    {
        Id = Guid.NewGuid().ToString();
        Status = JobStatus.Pending;
        Name = name ?? throw new ArgumentNullException(nameof(name));
        ExecutionTimeInSeconds = executionTimeInSeconds;
    }

    public string Id { get; }
    public string Name { get; }
    public int ExecutionTimeInSeconds { get; }
    public JobStatus Status { get; private set; }

    public void Take()
    {
        if (Status != JobStatus.Pending)
        {
            throw new InvalidOperationException(
                $"The job [{Name}] has already been started");
        }
        Status = JobStatus.Taken;
    }

    public Task RunningAsync()
    {
        if (Status != JobStatus.Taken)
        {
            throw new InvalidOperationException(
                $"The job [{Name}] has already been started");
        }
        Status = JobStatus.Running;

        return Task.Delay(ExecutionTimeInSeconds * 1000);
    }

    public Job Complete()
    {
        if (Status != JobStatus.Running)
        {
            throw new InvalidOperationException(
                $"The job [{Name}] has not been started or completed");
        }
        Status = JobStatus.Completed;

        return this;
    }
}