using App.Data;
using App.Models;
using App.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using System.Configuration;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration; 
// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages(); // Thêm dịch vụ Razor Pages

builder.Services.AddDbContext<AppDbContext>(options =>
{
    string connectString = builder.Configuration.GetConnectionString("DefaultConnection");
    options.UseMySql(connectString, ServerVersion.AutoDetect(connectString));
});

// Add Identity
builder.Services.AddIdentity<AppUser, IdentityRole>()
                .AddEntityFrameworkStores<AppDbContext >()
                .AddDefaultTokenProviders();
       

builder.Services.AddOptions();
var mailsetting = configuration.GetSection("MailSettings");
builder.Services.Configure<MailSettings>(mailsetting);
builder.Services.AddTransient<IEmailSender, SendMailService>(); // Gửi mail
//Identity Options
builder.Services.AddSingleton<IdentityErrorDescriber, AppIdentityErrorDescriber>();
//
builder.Services.AddTransient<CartServices>();
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ViewManageMenu", builder =>{
        builder.RequireAuthenticatedUser();
        builder.RequireRole(RoleName.Administrator);}
    );
    
});
builder.Services.Configure<Microsoft.AspNetCore.Identity.IdentityOptions>(options =>
{
    // Thiết lập về Password
    options.Password.RequireDigit = false; // Không bắt phải có số
    options.Password.RequireLowercase = false; // Không bắt phải có chữ thường
    options.Password.RequireNonAlphanumeric = false; // Không bắt ký tự đặc biệt
    options.Password.RequireUppercase = false; // Không bắt buộc chữ in
    options.Password.RequiredLength = 3; // Số ký tự tối thiểu của password
    options.Password.RequiredUniqueChars = 1; // Số ký tự riêng biệt

    // Cấu hình Lockout - khóa user
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5); // Khóa 5 phút
    options.Lockout.MaxFailedAccessAttempts = 5; // Thất bại 5 lầ thì khóa
    options.Lockout.AllowedForNewUsers = true;

    // Cấu hình về User.
    options.User.AllowedUserNameCharacters = // các ký tự đặt tên user
        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail = true;  // Email là duy nhất

    // Cấu hình đăng nhập.
    options.SignIn.RequireConfirmedEmail = false;            // Cấu hình xác thực địa chỉ email (email phải tồn tại)
    options.SignIn.RequireConfirmedPhoneNumber = false;     // Xác thực số điện thoại

});

// Google Service

builder.Services.AddAuthentication().AddGoogle(option =>
{
    var config = configuration.GetSection("Authentication:Google");
    option.ClientId = config["ClientId"];
    option.ClientSecret = config["ClientSecret"];
    option.CallbackPath = "/dang-nhap-tu-google";
    //localhost:.../signin-google
});

// Add Cookie
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/login/";
    options.LogoutPath = "/logout/";
    options.AccessDeniedPath = "/khongduoctruycap.html/";
    options.ExpireTimeSpan = TimeSpan.FromMinutes(30); // Kiểm tra thời gian hết hạn cookie
    options.SlidingExpiration = true; // Kiểm tra tính năng gia hạn cookie
});

builder.Services.AddDistributedMemoryCache();           // Đăng ký dịch vụ lưu cache trong bộ nhớ (Session sẽ sử dụng nó)
builder.Services.AddSession(cfg => {                    // Đăng ký dịch vụ Session
    cfg.Cookie.Name = "cart";             // Đặt tên Session - tên này sử dụng ở Browser (Cookie)
    cfg.IdleTimeout = new TimeSpan(0,30, 0);           // Thời gian tồn tại của Session
});

//build
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}


// contents/1.jpg => Uploads/1.jpg
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseStaticFiles(new StaticFileOptions(){
    FileProvider = new PhysicalFileProvider(
        Path.Combine(Directory.GetCurrentDirectory(), "Uploads")
        ),
    RequestPath = "/contents"
});

app.UseSession();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages(); // Thêm ánh xạ điểm cuối cho Razor Pages
// Map route cho Areas trước
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}"
);
app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

// Map route mặc định sau
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.Run();
