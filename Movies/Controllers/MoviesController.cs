using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Movies.Models;
using Movies.Services;

namespace Movies.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MoviesController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IMoviesService _moviesService;
        private readonly IGenresService _genresService;


        private new List<string> _allowedExtenstions = new List<string> { ".jpg", ".png" };
        private long _maxAllowedPosterSize = 1048576;

        public MoviesController(IMoviesService moviesService, IGenresService genresService, IMapper mapper)
        {
            _moviesService = moviesService;
            _genresService = genresService;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAsync()
        {
            var movies = await _moviesService.GetAll();
            //to do :: map movies to DTO

            var data = _mapper.Map<IEnumerable<MoviesDetailsDto>>(movies);

            return Ok(data);
        }


        [HttpGet("{id}")]
        public async Task<IActionResult>GetByIdAsync(int id)
        {
            var movie =await _moviesService.GetById(id);
            if (movie == null)
                return NotFound();

            var dto = _mapper.Map<MoviesDetailsDto>(movie);
            return Ok(dto);
        }



        [HttpGet("GetByGenreId")]
        public async Task<IActionResult> GetByGenreIdAsync(byte genreId)
        {
            var movies =await _moviesService.GetAll(genreId);
            //to do :: map movies to DTO

            var data = _mapper.Map<IEnumerable<MoviesDetailsDto>>(movies);

            return Ok(data);
        }

        [HttpPost]
        public async Task<ActionResult> CreateAsync([FromForm]MovieDto dto)
        {
            if (dto.Poster == null)
                return BadRequest("Poster is required!");

            if (!_allowedExtenstions.Contains(Path.GetExtension(dto.Poster.FileName).ToLower()))
                return BadRequest("Only .png and jpg images are allowed!");

            if (dto.Poster.Length > _maxAllowedPosterSize)
                return BadRequest(" Max allowed size is 1MB!");

            var isValidGenre = await _genresService.IsValidGenre(dto.GenreId);
            if (!isValidGenre)
                return BadRequest(" Invalid Genre Id!");

            using var datastream=new MemoryStream();
            await dto.Poster.CopyToAsync(datastream);

            var movie = _mapper.Map<Movie>(dto);
            movie.Poster=datastream.ToArray();



            _moviesService.Add(movie);
            return Ok(movie);

        }


        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAsync(int id, [FromForm] MovieDto dto)
        {
            var movie = await _moviesService.GetById(id);



            var isValidGenre = await _genresService.IsValidGenre(dto.GenreId);
            
            if (!isValidGenre)
                return BadRequest(" Invalid Genre Id!");

            if (dto.Poster != null)
            {
                if (!_allowedExtenstions.Contains(Path.GetExtension(dto.Poster.FileName).ToLower()))
                    return BadRequest("Only .png and jpg images are allowed!");

                if (dto.Poster.Length > _maxAllowedPosterSize)
                    return BadRequest(" Max allowed size is 1MB!");

                using var datastream = new MemoryStream();
                await dto.Poster.CopyToAsync(datastream); 

                movie.Poster = datastream.ToArray();
            };

            movie.Title = dto.Title;
            movie.Year = dto.Year;
            movie.GenreId = dto.GenreId;
            movie.StoreLine = dto.StoreLine;
            movie.Rate = dto.Rate;


            _moviesService.Update(movie);
            return Ok(movie);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAsync(int id)
        {
            var movie = await _moviesService.GetById(id);

            if (movie == null)
                return NotFound($"No Movie was found with ID:{id}");

            _moviesService.Delete(movie);

            return Ok(movie);
        }
    }
}
