using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RunGroopWebApp.Data;
using RunGroopWebApp.Interfaces;
using RunGroopWebApp.Models;
using RunGroopWebApp.Repository;
using RunGroopWebApp.Services;
using RunGroopWebApp.ViewModels;

namespace RunGroopWebApp.Controllers
{
    public class RaceController : Controller
    {
        private readonly IRaceRepository _raceRepository;
        private readonly IPhotoService _photoService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public RaceController(IRaceRepository raceRepository, IPhotoService photoService, IHttpContextAccessor contextAccessor)
        {
            _raceRepository = raceRepository;
            _photoService = photoService;   
            _httpContextAccessor = contextAccessor;
        }
        public async Task<IActionResult> Index()
        {
            IEnumerable<Race> races = await _raceRepository.GetAll();
            return View(races);
        }   

        public async Task<IActionResult> Detail(int Id)
        {
            Race race = await _raceRepository.GetByIdAsync(Id);
            return View(race);
        }

        public IActionResult Create()
        {
            var curUserId = HttpContext.User.GetUserId();
            var createRaceViewModel = new CreateRaceViewModel { AppUserId = curUserId };
            return View(createRaceViewModel);


        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateRaceViewModel raceVM)
        {
            if (ModelState.IsValid)
            {
                var result = await _photoService.AddPhotoAsync(raceVM.Image);

                var race = new Race
                {
                    Title = raceVM.Title,
                    Description = raceVM.Description,
                    Image = result.Url.ToString(),
                    RaceCategory = raceVM.RaceCategory,
                    AppUserId = raceVM.AppUserId,
                    Address = new Address
                    {
                        Street = raceVM.Address.Street,
                        City = raceVM.Address.City,
                        State = raceVM.Address.State,
                    }
                };
                _raceRepository.Add(race);
                //return RedirectToAction("Index");
                return RedirectToAction("Index", "Dashboard");
            }
            else
            {
                ModelState.AddModelError("", "Photo Upload Failed");

            }
            return View(raceVM);

        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var race = await _raceRepository.GetByIdAsync(id);
            if (race == null) return View("Error");

            var curUserId = HttpContext.User.GetUserId();


            var raceVM = new EditRaceViewModel
            {
                Title = race.Title,
                Description = race.Description,
                AddressId = race.AddressId,
                Address = race.Address,
                URL = race.Image,
                RaceCategory = race.RaceCategory,
                AppUserId = curUserId,
            };
            return View(raceVM);
        }


        [HttpPost]
        public async Task<IActionResult> Edit(int id, EditRaceViewModel raceVM)
        {
            //aditi
            //var curUserId = HttpContext.User.GetUserId();
            //raceVM.AppUserId = curUserId;
            //End Aditi

            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Failed to edit club");
                return View("Edit", raceVM);
            }

            var userRace = await _raceRepository.GetByIdAsyncNoTracking(id);

            if (userRace == null)
            {
                return View("Error");
            }

            if (userRace != null)
                try
                {
                    await _photoService.DeletePhotoAsync(userRace.Image);
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Could not delete photo");
                    return View(raceVM);
                }
            var photoResult = await _photoService.AddPhotoAsync(raceVM.Image);
            
            var race = new Race
            {
                Id = id,
                Title = raceVM.Title,
                Description = raceVM.Description,
                Image = photoResult.Url.ToString(),
                AddressId = raceVM.AddressId,
                Address = raceVM.Address,
                RaceCategory = raceVM.RaceCategory,
                AppUserId = raceVM.AppUserId,
            };

            _raceRepository.Update(race);
            //Aditi
            //ViewBag.SuccessMessage = "The club has been updated successfully!";
            TempData["AlertMessage"] = "Race updated successfully";
            //return RedirectToAction("Index");
            return RedirectToAction("Index", "Dashboard");

        }
        //GET
        public async Task<IActionResult> Delete(int id)
        {
            var RaceDetails = await _raceRepository.GetByIdAsync(id);
            if (RaceDetails == null) { return View("Delete"); }
            return View(RaceDetails);
        }

        //POST
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteRace(int id)
        {
            var RaceDetails = await _raceRepository.GetByIdAsync(id);
            if (RaceDetails == null) { return View("Error"); }
            _raceRepository.Delete(RaceDetails);

            TempData["AlertMessage"] = "Race deleted successfully";
            //return RedirectToAction("Index");
            return RedirectToAction("Index","Dashboard");
            
        }

    }
}
