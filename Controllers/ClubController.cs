using Microsoft.AspNetCore.Mvc;
using RunGroopWebApp.Data;
using RunGroopWebApp.Interfaces;
using RunGroopWebApp.Models;
using RunGroopWebApp.ViewModels;
using System.Diagnostics.Eventing.Reader;

namespace RunGroopWebApp.Controllers
{
    public class ClubController : Controller
    {
        private readonly IClubRepository _clubRepository;
        private readonly IPhotoService _photoService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ClubController(IClubRepository clubRepository, IPhotoService photoService, IHttpContextAccessor httpContextAccessor)
        {
            _clubRepository = clubRepository;
            _photoService = photoService;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<IActionResult> Index()
        {
            IEnumerable<Club> clubs = await _clubRepository.GetAll();
            return View(clubs);
        }

        public async Task<IActionResult> Detail(int Id)
        {
            Club club = await _clubRepository.GetByIdAsync(Id);
            return View(club);
        }

        public IActionResult Create()
        {
           var curUserId = HttpContext.User.GetUserId();
           var createClubViewModel = new CreateClubViewModel { AppUserId = curUserId };
           return View(createClubViewModel);

        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateClubViewModel clubVM)
        {
            if (ModelState.IsValid)
            {
                var result = await _photoService.AddPhotoAsync(clubVM.Image);

                var club = new Club
                {
                    Title = clubVM.Title,
                    Description = clubVM.Description,
                    Image = result.Url.ToString(),
                    ClubCategory = clubVM.ClubCategory,
                    AppUserId = clubVM.AppUserId,
                    Address = new Address
                    {
                        Street = clubVM.Address.Street,
                        City = clubVM.Address.City,
                        State = clubVM.Address.State,
                    }
                };
                _clubRepository.Add(club);
                //return RedirectToAction("Index");
                return RedirectToAction("Index", "Dashboard");
            }
            else
            {
                ModelState.AddModelError("", "Photo Upload Failed");

            }
            return View(clubVM);

        }
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var club = await _clubRepository.GetByIdAsync(id);
            if (club == null) return View("Error");

            var curUserId = HttpContext.User.GetUserId();


            var clubVM = new EditClubViewModel
            {
                Title = club.Title,
                Description = club.Description,
                AddressId = club.AddressId,
                Address = club.Address,
                URL = club.Image,
                ClubCategory = club.ClubCategory,
                AppUserId = curUserId,
            };
            return View(clubVM);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, EditClubViewModel clubVM)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Failed to edit club");
                return View("Edit", clubVM);
            }

            var userClub = await _clubRepository.GetByIdAsyncNoTracking(id);

            if (userClub == null)
            {
                return View("Error");
            }

            if (userClub != null)
                try
                {
                    await _photoService.DeletePhotoAsync(userClub.Image);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Could not delete photo");
                    return View(clubVM);
                }
            var photoResult = await _photoService.AddPhotoAsync(clubVM.Image);

            var club = new Club
            {
                Id = id,
                Title = clubVM.Title,
                Description = clubVM.Description,
                Image = photoResult.Url.ToString(),
                AddressId = clubVM.AddressId,
                Address = clubVM.Address,
                ClubCategory = clubVM.ClubCategory,
                AppUserId = clubVM.AppUserId,
            };

            _clubRepository.Update(club);
            //Aditi
            //ViewBag.SuccessMessage = "The club has been updated successfully!";
            TempData["AlertMessage"] = "Club updated successfully";
            // return RedirectToAction("Index");
            return RedirectToAction("Index", "Dashboard");

        }
        //GET
        public async Task<IActionResult> Delete(int id)
        { 
            var ClubDetails = await _clubRepository.GetByIdAsync(id);
            if(ClubDetails == null) { return View("Delete"); }
            return View(ClubDetails);
        }

        //POST
        [HttpPost, ActionName("Delete")]
       
        public async Task<IActionResult> DeleteClub(int id)
        {
            var ClubDetails = await _clubRepository.GetByIdAsync(id);
            if (ClubDetails == null) { return View("Error"); }
            _clubRepository.Delete(ClubDetails);

            TempData["AlertMessage"] = "Club deleted successfully";
            //return RedirectToAction("Index");
            //return View(ClubDetails);
            return RedirectToAction("Index", "Dashboard");
        }
        
    }

}   
