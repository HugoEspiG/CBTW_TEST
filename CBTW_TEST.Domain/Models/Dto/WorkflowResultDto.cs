using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CBTW_TEST.Domain.Models.Dto
{
    public class WorkflowResultDto
    {
        public bool WasSuccess { get; set; }
        public object? Result { get; set; }
    }
}
