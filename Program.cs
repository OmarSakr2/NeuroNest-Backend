using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Text.Json.Serialization;
using AustimAPI.Models;
using AustimAPI.Middleware;
using AustimAPI.Services;
using Microsoft.AspNetCore.Http.Features;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<apiDBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    options.JsonSerializerOptions.WriteIndented = true;
});

builder.Services.AddHttpClient();
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<AIService>();


builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 500L * 1024 * 1024;
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

var key = builder.Configuration["Jwt:Key"];
if (string.IsNullOrEmpty(key))
    throw new Exception("JWT Key مش موجود في appsettings.json");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"],
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
    };
});

builder.Services.AddAuthorization();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Autism Detection API", Version = "v1" });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "ادخل: Bearer {token}",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {{
        new OpenApiSecurityScheme
        {
            Reference = new OpenApiReference
            {
                Type = ReferenceType.SecurityScheme,
                Id = "Bearer"
            }
        },
        new string[] {}
    }});
});

var app = builder.Build();

// ✅ Migration + Seed البيانات
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<apiDBContext>();
    db.Database.Migrate();

    // ✅ أضف الـ 20 سؤال لو الجدول فاضي
    if (!db.Question.Any())
    {
        db.Question.AddRange(
            new Question
            {
                QuestionNumber = 1,
                RiskIfNo = true,
                IsActive = true,
                QuestionText_AR = "لو شاورت على حاجة في الناحية التانية من الأوضة، هل طفلك بيبص عليها؟",
                QuestionText_EN = "If you point at something across the room, does your child look at it?"
            },

            new Question
            {
                QuestionNumber = 2,
                RiskIfNo = false,
                IsActive = true,
                QuestionText_AR = "هل شكيت قبل كده إن طفلك ممكن يكون مبيسمعش؟",
                QuestionText_EN = "Have you ever wondered if your child might be deaf?"
            },

            new Question
            {
                QuestionNumber = 3,
                RiskIfNo = true,
                IsActive = true,
                QuestionText_AR = "هل طفلك بيلعب ألعاب تخيلية؟",
                QuestionText_EN = "Does your child play pretend or make-believe?"
            },

            new Question
            {
                QuestionNumber = 4,
                RiskIfNo = true,
                IsActive = true,
                QuestionText_AR = "هل طفلك بيحب يتسلق الأشياء؟",
                QuestionText_EN = "Does your child like climbing on things?"
            },

            new Question
            {
                QuestionNumber = 5,
                RiskIfNo = false,
                IsActive = true,
                QuestionText_AR = "هل طفلك بيعمل حركات غريبة بصوابعه قريب من عينه؟",
                QuestionText_EN = "Does your child make unusual finger movements near his or her eyes?"
            },

            new Question
            {
                QuestionNumber = 6,
                RiskIfNo = true,
                IsActive = true,
                QuestionText_AR = "هل طفلك بيشاور بصباعه عشان يطلب حاجة أو يطلب المساعدة؟",
                QuestionText_EN = "Does your child point with one finger to ask for something or to get help?"
            },

            new Question
            {
                QuestionNumber = 7,
                RiskIfNo = true,
                IsActive = true,
                QuestionText_AR = "هل طفلك بيشاور بصباعه عشان يوريك حاجة تلفت الانتباه؟",
                QuestionText_EN = "Does your child point with one finger to show you something interesting?"
            },

            new Question
            {
                QuestionNumber = 8,
                RiskIfNo = true,
                IsActive = true,
                QuestionText_AR = "هل طفلك بيهتم بالأطفال التانيين؟",
                QuestionText_EN = "Is your child interested in other children?"
            },

            new Question
            {
                QuestionNumber = 9,
                RiskIfNo = true,
                IsActive = true,
                QuestionText_AR = "هل طفلك بيجيبلك حاجات يوريها لك عشان يشاركك إياها؟",
                QuestionText_EN = "Does your child show you things by bringing them to you or holding them up for you to see?"
            },

            new Question
            {
                QuestionNumber = 10,
                RiskIfNo = true,
                IsActive = true,
                QuestionText_AR = "هل طفلك بيرد لما تنده على اسمه؟",
                QuestionText_EN = "Does your child respond when you call his or her name?"
            },

            new Question
            {
                QuestionNumber = 11,
                RiskIfNo = true,
                IsActive = true,
                QuestionText_AR = "لما تبتسم لطفلك، هل بيبتسم لك هو كمان؟",
                QuestionText_EN = "When you smile at your child, does he or she smile back at you?"
            },

            new Question
            {
                QuestionNumber = 12,
                RiskIfNo = false,
                IsActive = true,
                QuestionText_AR = "هل طفلك بينزعج جداً من الأصوات العادية؟",
                QuestionText_EN = "Does your child get upset by everyday noises?"
            },

            new Question
            {
                QuestionNumber = 13,
                RiskIfNo = true,
                IsActive = true,
                QuestionText_AR = "هل طفلك بيقدر يمشي لوحده؟",
                QuestionText_EN = "Does your child walk?"
            },

            new Question
            {
                QuestionNumber = 14,
                RiskIfNo = true,
                IsActive = true,
                QuestionText_AR = "هل طفلك بيبص في عينك لما تتكلم أو تلعب معاه؟",
                QuestionText_EN = "Does your child look you in the eye when you are talking to him or her?"
            },

            new Question
            {
                QuestionNumber = 15,
                RiskIfNo = true,
                IsActive = true,
                QuestionText_AR = "هل طفلك بيحاول يقلد حركاتك؟",
                QuestionText_EN = "Does your child try to copy what you do?"
            },

            new Question
            {
                QuestionNumber = 16,
                RiskIfNo = true,
                IsActive = true,
                QuestionText_AR = "لو لفيت راسك فجأة تبص على حاجة، هل طفلك بيبص معاك؟",
                QuestionText_EN = "If you turn your head to look at something, does your child look around to see what you are looking at?"
            },

            new Question
            {
                QuestionNumber = 17,
                RiskIfNo = true,
                IsActive = true,
                QuestionText_AR = "هل طفلك بيحاول يخليك تبص عليه وتتابعه وهو بيلعب؟",
                QuestionText_EN = "Does your child try to get you to watch him or her?"
            },

            new Question
            {
                QuestionNumber = 18,
                RiskIfNo = true,
                IsActive = true,
                QuestionText_AR = "هل طفلك بيفهمك لما تطلب منه يعمل حاجة بسيطة؟",
                QuestionText_EN = "Does your child understand when you tell him or her to do something?"
            },

            new Question
            {
                QuestionNumber = 19,
                RiskIfNo = true,
                IsActive = true,
                QuestionText_AR = "لو حصلت حاجة غريبة، هل طفلك بيبص في وشك عشان يشوف رد فعلك؟",
                QuestionText_EN = "If something new happens, does your child look at your face to see how you feel about it?"
            },

            new Question
            {
                QuestionNumber = 20,
                RiskIfNo = true,
                IsActive = true,
                QuestionText_AR = "هل طفلك بيحب الأنشطة الحركية؟",
                QuestionText_EN = "Does your child like movement activities?"
            }
        );
        db.SaveChanges();
    }
}

app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseStaticFiles();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();