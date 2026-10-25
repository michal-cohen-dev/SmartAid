using BLL.MappingProfiles;
using DAL.Repositories;
using DAL.Repositories.Interfaces;
using DAL;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using BLL.Services;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// הגדרת תיקיית התמונות
var imagesFolder = Path.GetFullPath(Path.Combine(
    builder.Environment.ContentRootPath,
    builder.Configuration["ImagesFolder"] ?? "../Images"));

Directory.CreateDirectory(imagesFolder);

// רישום ה-DbContext למסד הנתונים
var connStr = builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrWhiteSpace(connStr))
{
    throw new InvalidOperationException(
        "Connection string 'DefaultConnection' is missing. " +
        "Set it with an environment variable.");
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connStr));

// רישום ה-AutoMapper בצורה נפרדת ותקינה
builder.Services.AddAutoMapper(cfg => {
    cfg.AddProfile<MappingProfile>();
});

// רישום 7 ה-Repositories ב-Dependency Injection (AddScoped)
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IVolunteerRepository, VolunteerRepository>();
builder.Services.AddScoped<IEquipmentTypeRepository, EquipmentTypeRepository>();
builder.Services.AddScoped<IVolunteerEquipmentRepository, VolunteerEquipmentRepository>();
builder.Services.AddScoped<IHelpRequestRepository, HelpRequestRepository>();
builder.Services.AddScoped<IRequestEquipmentRepository, RequestEquipmentRepository>();
builder.Services.AddScoped<IRequestOfferRepository, RequestOfferRepository>();
builder.Services.AddScoped<IVolunteerService, VolunteerService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IHelpRequestService, HelpRequestService>();
builder.Services.AddScoped<IRequestEquipmentService, RequestEquipmentService>();
builder.Services.AddScoped<IRequestOfferService, RequestOfferService>();
builder.Services.AddScoped<IEquipmentTypeService, EquipmentTypeService>();
builder.Services.AddScoped<IVolunteerEquipmentService, VolunteerEquipmentService>();
builder.Services.AddSingleton<IImageService>(
    new ImageService(imagesFolder));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(imagesFolder),
    RequestPath = "/images"
});
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();