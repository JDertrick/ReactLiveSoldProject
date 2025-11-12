using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using ReactLiveSoldProject.ServerBL.Base;
using ReactLiveSoldProject.ServerBL.DTOs;
using ReactLiveSoldProject.ServerBL.Infrastructure.Interfaces;
using System.Collections.Generic;

namespace ReactLiveSoldProject.ServerBL.Infrastructure.Services
{
    public class UserService : IUserService
    {
        private readonly LiveSoldDbContext _dbContext;
        private readonly IMapper _mapper;

        public UserService(LiveSoldDbContext dbContext, IMapper mapper)
        {
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<ICollection<UserProfileDto>> GetUserAsync(Guid organizationId)
        {
            var users = _dbContext.Organizations.Where(o => o.Id == organizationId)
                .SelectMany(o => o.Members)
                .Select(m => m.User)
                .ProjectTo<UserProfileDto>(_mapper.ConfigurationProvider);

            return await users.ToArrayAsync();
        }
    }
}
