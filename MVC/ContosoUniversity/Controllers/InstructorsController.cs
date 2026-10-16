using ContosoUniversity.Data;
using ContosoUniversity.Models;
using ContosoUniversity.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace ContosoUniversity.Controllers
{
    public class InstructorsController : Controller
    {
        private readonly ContosoUniversityContext _context;

        public InstructorsController(ContosoUniversityContext context)
        {
            _context = context;
        }

        // GET: Instructors
        public async Task<IActionResult> Index(int? id, int? courseID)
        {
           
           ContosoUniversity.Models.ViewModels.InstructorIndexData viewModel = new Models.ViewModels.InstructorIndexData();
            viewModel.Instructors = await _context.Instructors
                .Include(i => i.OfficeAssignment)
                .Include(i => i.CourseAssignments)
                    .ThenInclude(ca => ca.Course)
                        .ThenInclude(c => c.Enrollments)
                            .ThenInclude(e => e.Student)
                .Include(i=>i.CourseAssignments)
                    .ThenInclude(ca => ca.Course)
                        .ThenInclude(c => c.Department)
                .AsNoTracking()
                .OrderBy(i=>i.LastName)
                .ToListAsync();

            if (id != null)
            {
                ViewData["InstructorID"] = id.Value;
                Instructor instructor = viewModel.Instructors.Where (i => i.ID == id.Value).Single();
                viewModel.Courses=instructor.CourseAssignments.Select(ca => ca.Course);
            }

            if (courseID != null)
            {
                ViewData["CourseID"] = courseID.Value;
                viewModel.Enrollments = viewModel.Courses.Where(c => c.CourseID == courseID)
                    .Single().Enrollments;
            }

            //return View(await _context.Instructors.ToListAsync());
            return View(viewModel);
        }

        // GET: Instructors/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var instructor = await _context.Instructors
                .Include(i=>i.OfficeAssignment)
                .FirstOrDefaultAsync(m => m.ID == id);
            if (instructor == null)
            {
                return NotFound();
            }

            return View(instructor);
        }

        // GET: Instructors/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Instructors/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,LastName,FirstName,HireDate")] Instructor instructor)
        {
            if (ModelState.IsValid)
            {
                _context.Add(instructor);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(instructor);
        }

        // GET: Instructors/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var instructor = await _context
                                    .Instructors
                                    .Include(i => i.OfficeAssignment)
                                    .Include(i => i.CourseAssignments)
                                    .ThenInclude(ca => ca.Course)
                                    .AsNoTracking()
                                    .FirstOrDefaultAsync(i => i.ID == id);
            if (instructor == null)
            {
                return NotFound();
            }
            
            PopulateAssignmendCourseData(instructor);
            return View(instructor);
        }

        // POST: Instructors/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        //public async Task<IActionResult> Edit(int id, [Bind("ID,LastName,FirstName,HireDate")] Instructor instructor)
        public async Task<IActionResult> Edit(int id, string[] selectedCourses)
        {
            //if (id != null)
            //{
            //    return NotFound();
            //}

            Instructor instructor = await _context.Instructors
                                    
                                    .Include(i => i.OfficeAssignment)
                                    .Include(i => i.CourseAssignments)
                                        .ThenInclude(ca => ca.Course)
                                    .FirstOrDefaultAsync(i => i.ID == id);

            /*if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(instructor);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!InstructorExists(instructor.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }*/

            Task<bool> ok = TryUpdateModelAsync<Instructor>
                (
                instructor,
                "",
                i => i.FirstName, i => i.LastName, i => i.HireDate, i => i.OfficeAssignment

                );

            if (ok.IsCompleted)
            {
                if(String.IsNullOrWhiteSpace(instructor.OfficeAssignment?.Location)) 
                    instructor.OfficeAssignment = null;

                UpdateInstructorCourses(selectedCourses, instructor);

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException ex)
                {
                    ModelState.AddModelError("", "Невозможно обновить данные");
                    //return View(instructor);
                }

            }
            UpdateInstructorCourses(selectedCourses, instructor);
            PopulateAssignmendCourseData(instructor);
            return Redirect($"/instructors/Details/{instructor.ID}");
            return View(instructor);
            
        }

        // GET: Instructors/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var instructor = await _context.Instructors
                .FirstOrDefaultAsync(m => m.ID == id);
            if (instructor == null)
            {
                return NotFound();
            }

            return View(instructor);
        }

        // POST: Instructors/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var instructor = await _context.Instructors.FindAsync(id);
            if (instructor != null)
            {
                _context.Instructors.Remove(instructor);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool InstructorExists(int id)
        {
            return _context.Instructors.Any(e => e.ID == id);
        }

		void PopulateAssignmendCourseData(Instructor instructor)
		{
			DbSet<Course> courses = _context.Courses;
			HashSet<int> assignedCourses = new HashSet<int>
				(
				instructor.CourseAssignments.Select(c => c.CourseID)
				);
			List<AssignedCourseData> viewModel = new List<AssignedCourseData>();
			foreach (Course course in courses)
			{
				viewModel.Add
				(
					new AssignedCourseData
					{
						CourseID = course.CourseID,
						Title = course.Title,
						Assigned = assignedCourses.Contains(course.CourseID)

					}
				);
			}
			ViewData["Courses"] = viewModel;
		}

		void UpdateInstructorCourses(string[] selectedCourses, Instructor instructor)
		{
			if (selectedCourses == null)
			{
				instructor.CourseAssignments = new List<CourseAssignment>();
				return;
			}

            Course[] allCourses=_context.Courses.ToArray();
            Course[] courses=new Course[selectedCourses.Length];
            ICollection<CourseAssignment> courseAssignments = new List<CourseAssignment>();// [selectedCourses.Length];
            
            for(int i=0; i<courses.Length; i++)
            {
                courses[i] = _context.Courses.Where(c => c.CourseID.ToString() == selectedCourses[i]).First();
                courseAssignments.Add(new CourseAssignment { CourseID = courses[i].CourseID, InstructorID = instructor.ID });
                //courseAssignments[i] = new CourseAssignment { CourseID = courses[i].CourseID, InstructorID = instructor.ID };
            }

            instructor.CourseAssignments = courseAssignments;
            return;

			HashSet<string> selectedCoursesHS = new HashSet<string>(selectedCourses);
			HashSet<int> instructorCoursesHS = new HashSet<int>
				(
			        instructor.CourseAssignments.Select(c => c.Course.CourseID)
				);

			foreach (Course course in _context.Courses)

			{
				if (selectedCoursesHS.Contains(course.CourseID.ToString()))
					if (!instructorCoursesHS.Contains(course.CourseID))
					{
						instructor.CourseAssignments.Add
						(
							new CourseAssignment
							{
								InstructorID = instructor.ID,
								CourseID = course.CourseID,
							}
						);
					}
					else
					{
						if (instructorCoursesHS.Contains(course.CourseID))
						{
							CourseAssignment courseToRemove =
								instructor
								.CourseAssignments
								.FirstOrDefault(i => i.CourseID == course.CourseID);
							_context.Remove(courseToRemove);
						}
					}
			}
		}
	}
}

