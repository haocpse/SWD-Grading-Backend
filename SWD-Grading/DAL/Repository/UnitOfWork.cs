using DAL.Interface;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Repository
{
	public class UnitOfWork : IUnitOfWork
	{
		private readonly SWDGradingDbContext _context;
		private readonly Hashtable _repos = new();

		public UnitOfWork(SWDGradingDbContext context)
		{
			_context = context;
		}
		public IGenericRepository<T, TId> GetRepository<T, TId>()
			where T : class
			where TId : notnull
		{
			var typeName = typeof(T).Name;
			if (_repos.ContainsKey(typeName))
				return (IGenericRepository<T, TId>)_repos[typeName]!;

			var repoInstance = new GenericRepository<T, TId>(_context);
			_repos.Add(typeName, repoInstance);
			return repoInstance;
		}

		public async Task<int> SaveChangesAsync()
			=> await _context.SaveChangesAsync();

		public async Task<IDbContextTransaction> BeginTransactionAsync()
			=> await _context.Database.BeginTransactionAsync();

		private IUserRepository? _userRepository;
		public IUserRepository UserRepository
		{
			get
			{
				if (_userRepository == null)
				{
					_userRepository = new UserRepository(_context);
				}
				return _userRepository;
			}
		}

		private IStudentRepository? _studentRepository;
		public IStudentRepository StudentRepository
		{
			get
			{
				if (_studentRepository == null)
				{
					_studentRepository = new StudentRepository(_context);
				}
				return _studentRepository;
			}
		}

		private IExamRepository? _examRepository;
		public IExamRepository ExamRepository
		{
			get
			{
				if (_examRepository == null)
				{
					_examRepository = new ExamRepository(_context);
				}
				return _examRepository;
			}
		}

		private IExamZipRepository? _examZipRepository;
		public IExamZipRepository ExamZipRepository
		{
			get
			{
				if (_examZipRepository == null)
				{
					_examZipRepository = new ExamZipRepository(_context);
				}
				return _examZipRepository;
			}
		}

		private IExamStudentRepository? _examStudentRepository;
		public IExamStudentRepository ExamStudentRepository
		{
			get
			{
				if (_examStudentRepository == null)
				{
					_examStudentRepository = new ExamStudentRepository(_context);
				}
				return _examStudentRepository;
			}
		}

		private IDocFileRepository? _docFileRepository;
		public IDocFileRepository DocFileRepository
		{
			get
			{
				if (_docFileRepository == null)
				{
					_docFileRepository = new DocFileRepository(_context);
				}
				return _docFileRepository;
			}
		}

		private ISimilarityCheckRepository? _similarityCheckRepository;
		public ISimilarityCheckRepository SimilarityCheckRepository
		{
			get
			{
				if (_similarityCheckRepository == null)
				{
					_similarityCheckRepository = new SimilarityCheckRepository(_context);
				}
				return _similarityCheckRepository;
			}
		}
		private ISimilarityResultRepository? _similarityResultRepository;
		public ISimilarityResultRepository SimilarityResultRepository
		{
			get
			{
				if (_similarityResultRepository == null)
				{
					_similarityResultRepository = new SimilarityResultRepository(_context);
				}
				return _similarityResultRepository;
			}
		}

		private IExamQuestionRepository _examQuestionRepository;
		public IExamQuestionRepository ExamQuestionRepository
		{
			get
			{
				if (_examQuestionRepository == null)
				{
					_examQuestionRepository = new ExamQuestionRepository(_context);
				}
				return _examQuestionRepository;
			}
		}

		private IRubricRepository _rubricRepository;
		public IRubricRepository RubricRepository
		{
			get
			{
				if (_rubricRepository == null)
				{
					_rubricRepository = new RubricRepository(_context);
				}
				return _rubricRepository;
			}
		}

		private IGradeDetailRepository _gradeDetailRepository;
		public IGradeDetailRepository GradeDetailRepository
		{
			get
			{
				if (_gradeDetailRepository == null)
				{
					_gradeDetailRepository = new GradeDetailRepository(_context);
				}
				return _gradeDetailRepository;
			}
		}

		private IGradeRepository _gradeRepository;
		public IGradeRepository GradeRepository
		{
			get
			{
				if (_gradeRepository == null)
				{
					_gradeRepository = new GradeRepository(_context);
				}
				return _gradeRepository;
			}
		}

		private IGradeExportRepository _gradeExportRepository;
		public IGradeExportRepository GradeExportRepository
		{
			get
			{
				if (_gradeExportRepository == null)
				{
					_gradeExportRepository = new GradeExportRepository(_context);
				}
				return _gradeExportRepository;
			}
		}

	}
}
