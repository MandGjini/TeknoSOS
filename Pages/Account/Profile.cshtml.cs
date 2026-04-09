using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using TeknoSOS.WebApp.Domain.Entities;

namespace TeknoSOS.WebApp.Pages.Account
{
    [Authorize]
    public class ProfileModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public ProfileModel(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public ApplicationUser? CurrentUser { get; set; }
        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            CurrentUser = await _userManager.GetUserAsync(User);
            if (CurrentUser == null)
            {
                return RedirectToPage("/Index");
            }

            SuccessMessage = TempData["Success"]?.ToString();
            ErrorMessage = TempData["Error"]?.ToString();
            
            return Page();
        }

        public async Task<IActionResult> OnPostUpdateProfileAsync(string firstName, string lastName, string? phoneNumber, string? address, string? city, string? postalCode)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToPage("/Index");
            }

            user.FirstName = firstName;
            user.LastName = lastName;
            user.PhoneNumber = phoneNumber;
            user.Address = address;
            user.City = city;
            user.PostalCode = postalCode;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                TempData["Success"] = "Profili u përditësua me sukses!";
            }
            else
            {
                TempData["Error"] = "Gabim gjatë përditësimit të profilit: " + string.Join(", ", result.Errors.Select(e => e.Description));
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostUpdateProfessionalInfoAsync(string? companyName, int? yearsOfExperience, string? bio, int? serviceRadiusKm, bool isAvailable = false)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToPage("/Index");
            }

            user.CompanyName = companyName;
            user.YearsOfExperience = yearsOfExperience;
            user.Bio = bio;
            user.ServiceRadiusKm = serviceRadiusKm;
            user.IsAvailableForWork = isAvailable;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                TempData["Success"] = "Informacioni profesional u përditësua me sukses!";
            }
            else
            {
                TempData["Error"] = "Gabim gjatë përditësimit: " + string.Join(", ", result.Errors.Select(e => e.Description));
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostChangePasswordAsync(string currentPassword, string newPassword, string confirmPassword)
        {
            if (newPassword != confirmPassword)
            {
                TempData["Error"] = "Fjalëkalimet e reja nuk përputhen!";
                return RedirectToPage();
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToPage("/Index");
            }

            var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);
            if (result.Succeeded)
            {
                await _signInManager.RefreshSignInAsync(user);
                TempData["Success"] = "Fjalëkalimi u ndryshua me sukses!";
            }
            else
            {
                TempData["Error"] = "Gabim: " + string.Join(", ", result.Errors.Select(e => e.Description));
            }

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDeleteAccountAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToPage("/Index");
            }

            await _signInManager.SignOutAsync();
            var result = await _userManager.DeleteAsync(user);
            
            if (result.Succeeded)
            {
                return RedirectToPage("/Index");
            }

            TempData["Error"] = "Gabim gjatë fshirjes së llogarisë.";
            return RedirectToPage();
        }
    }
}
