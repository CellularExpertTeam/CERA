using Defencev1.Models;
using Defencev1.Utils.Result;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Defencev1.Services.Cells;

public interface ICellService
{
    Task<Result<string>> AddNewCellToWorkspace(long workspaceId, CellAddRequest cell);
}
