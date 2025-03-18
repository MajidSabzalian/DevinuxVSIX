using DevinuxVSIXGenerator.Forms;
using EnvDTE;
using System;
using System.Linq;

namespace DevinuxVSIXGenerator
{
    public class DDDArchitectoriesInformation
    {
        public const string Domain = "Domain";
        public const string Application = "Application";
        public const string Persistence = "Persistence";
        public const string Infrastructure = "Infrastructure";
        public const string Common = "Common";
    }
    public class CreateDddFoldersCommands : DevinuxCommand
    {
        public override int MenuCommandId { get => PackageIds.CreateDDDFolders; }
        private readonly DTE dte;
        public CreateDddFoldersCommands(DTE _dte)
        {
            dte = _dte;
        }
        public override void OnClick(object sender, EventArgs e)
        {
            var folderNames = new[] {
                DDDArchitectoriesInformation.Domain ,
                DDDArchitectoriesInformation.Application ,
                DDDArchitectoriesInformation.Common ,
                DDDArchitectoriesInformation.Infrastructure ,
                DDDArchitectoriesInformation.Persistence
            };
            foreach (var folderName in folderNames.Where(m => m.Trim().Length > 0))
            {
                var selectedItem = dte.GetSelectedItem();
                var folderPath = GetActiveProjectFolderPath() + "\\" + folderName;
                if (!System.IO.Directory.Exists(folderPath))
                {
                    System.IO.Directory.CreateDirectory(folderPath);
                    dte.GetSelectedItem().ProjectItems.AddFromDirectory(folderPath);
                }
            }
        }
    }
    public class CreateApplicationServiceCommands : DevinuxCommand
    {
        public override int MenuCommandId { get => PackageIds.CreateApplicationService; }
        public CreateApplicationServiceCommands(DTE _dte)
        {
            dte = _dte;
        }
        public override void OnClick(object sender, EventArgs e)
        {
            string selectedText = GetSelectionText();
            string _using = @"
using Devinux.AutoDI;
";
            var _props = GetEntitiesFromClassDefinitions(selectedText);
            if (_props != null && _props.Count() > 0)
            {
                foreach (var model in _props)
                {
                    var NameSpace = GetActiveProjectNameSpace();
                    // insert command
                    {
                        $@"
    using AutoMapper;
    using {NameSpace}.Application.Contracts.Repositories;
    using {NameSpace}.Common.Application.CommandPattern;
    using {NameSpace}.Common.Application.ResponseModel.Genereic;
{_using}

    namespace {NameSpace}.Application.Services.{model.Name}.Commands.Insert;

    [DevinuxServiceScope]
    public class InsertCommandHandler : ICommandHandler<InsertCommand, InsertCommandDto>
    {{
        private readonly IMapper _mapper;
        private readonly I{model.Name}CommandRepository _cmdRepo;
        public InsertCommandHandler(I{model.Name}CommandRepository cmdRepo,IMapper mapper)
        {{
            _cmdRepo = cmdRepo;
            _mapper = mapper;
        }}

        public async Task<InsertCommandDto> Handle(InsertCommand command, CancellationToken ct)
        {{
            var _model = _mapper.Map<Domain.Entities.{model.Name}>(command);
            var result = await _cmdRepo.InsertAsync(_model, ct);
            return new InsertCommandDto() {{ Result = result > 0e }};
        }}
    }}
    ".SaveFile($"{GetActiveProjectFolderPath()}//{DDDArchitectoriesInformation.Application}//Services//{model.Name}//Commands//Insert//InsertCommandHandler.cs"); ;
                    }
                    // update command
                    {
                        $@"
    using AutoMapper;
    using {NameSpace}.Application.Contracts.Repositories;
    using {NameSpace}.Common.Application.CommandPattern;
    using {NameSpace}.Common.Application.ResponseModel.Genereic;
{_using}

    namespace {NameSpace}.{DDDArchitectoriesInformation.Application}.Services.{model.Name}.Commands.Update;

    [DevinuxServiceScope]
    public class UpdateCommandHandler : ICommandHandler<UpdateCommand, UpdateCommandDto>
    {{
        private readonly IMapper _mapper;
        private readonly I{model.Name}CommandRepository _cmdRepo;
        public UpdateCommandHandler(I{model.Name}CommandRepository cmdRepo,IMapper mapper)
        {{
            _cmdRepo = cmdRepo;
            _mapper = mapper;
        }}

        public async Task<UpdateCommandDto> Handle(UpdateCommand command, CancellationToken ct)
        {{
            var _model = _mapper.Map<Domain.Entities.{model.Name}>(command);
            var result = await _cmdRepo.UpdateAsync(_model, ct);
            return new UpdateCommandDto() {{ Result = result > 0 }};
        }}
    }}
    ".SaveFile($"{GetActiveProjectFolderPath()}//{DDDArchitectoriesInformation.Application}//Services//{model.Name}//Commands//Update//UpdateCommandHandler.cs"); ;
                    }
                    // delete command
                    {
                        $@"
    using AutoMapper;
    using {NameSpace}.Application.Contracts.Repositories;
    using {NameSpace}.Common.Application.CommandPattern;
    using {NameSpace}.Common.Application.ResponseModel.Genereic;
{_using}

    namespace {NameSpace}.{DDDArchitectoriesInformation.Application}.Services.{model.Name}.Commands.Delete;
    [DevinuxServiceScope]
    public class DeleteCommandHandler : ICommandHandler<DeleteCommand, DeleteCommandDto>
    {{
        private readonly IMapper _mapper;
        private readonly I{model.Name}CommandRepository _cmdRepo;
        public DeleteCommandHandler(I{model.Name}CommandRepository cmdRepo,IMapper mapper)
        {{
            _cmdRepo = cmdRepo;
            _mapper = mapper;
        }}

        public async Task<DeleteCommandDto> Handle(DeleteCommand command, CancellationToken ct)
        {{
            var result = await _cmdRepo.DeleteAsync(command.Id, ct);
            return new DeleteCommandDto() {{ Result = result > 0 }};
        }}
    }}
    ".SaveFile($"{GetActiveProjectFolderPath()}//{DDDArchitectoriesInformation.Application}//Services//{model.Name}//Commands//Delete//DeleteCommandHandler.cs"); ;
                    }
                    // select query
                    {
                        $@"
    using AutoMapper;
    using {NameSpace}.Application.Contracts.Repositories;
    using {NameSpace}.Common.Application.CommandPattern;
    using {NameSpace}.Common.Application.ResponseModel.Genereic;
{_using}

    namespace {NameSpace}.{DDDArchitectoriesInformation.Application}.Services.{model.Name}.Queries.Select;
    [DevinuxServiceScope]
    public class SelectQueryHandler : IQueryHandler<SelectQuery, SelectQueryDto>
    {{
        private readonly IMapper _mapper;
        private readonly I{model.Name}QueryRepository _qryRepo;
        public SelectQueryHandler(I{model.Name}QueryRepository qryRepo,IMapper mapper)
        {{
            _qryRepo = qryRepo;
            _mapper = mapper;
        }}

        public async Task<SelectQueryDto> Handle(SelectQuery query, CancellationToken ct)
        {{
            var _model = await _qryRepo.SelectAsync(query.Id , ct);
            return _mapper.Map<SelectQueryDto>(_model);
        }}
    }}
    ".SaveFile($"{GetActiveProjectFolderPath()}//{DDDArchitectoriesInformation.Application}//Services//{model.Name}//Queries//Select//SelectQueryHandler.cs"); ;
                    }
                    // List query
                    {
                        $@"
    using AutoMapper;
    using {NameSpace}.Application.Contracts.Repositories;
    using {NameSpace}.Common.Application.CommandPattern;
    using {NameSpace}.Common.Application.ResponseModel.Genereic;
{_using}

    namespace {NameSpace}.{DDDArchitectoriesInformation.Application}.Services.{model.Name}.Queries.List;
    [DevinuxServiceScope]
    public class ListQueryHandler : IQueryHandler<ListQuery, List<ListQueryDto>>
    {{
        private readonly IMapper _mapper;
        private readonly I{model.Name}QueryRepository _qryRepo;
        public ListQueryHandler(I{model.Name}QueryRepository qryRepo,IMapper mapper)
        {{
            _qryRepo = qryRepo;
            _mapper = mapper;
        }}

        public async Task<List<ListQueryDto>> Handle(ListQuery query, CancellationToken ct)
        {{
            var _model = await _qryRepo.ListAsync(query.Page , query.PageSize , ct);
            return _model.Select(x => _mapper.Map<ListQueryDto>(x)).ToList();
        }}
    }}
    ".SaveFile($"{GetActiveProjectFolderPath()}//{DDDArchitectoriesInformation.Application}//Services//{model.Name}//Queries//List//ListQueryHandler.cs"); ;
                    }
                }
            }
            else
            {
                "selection text is not class".ShowMessage();
            }
        }
    }
    public class CreateApplicationModelCommands : DevinuxCommand
    {
        public override int MenuCommandId { get => PackageIds.CreateApplicationModel; }
        public CreateApplicationModelCommands(DTE _dte)
        {
            dte = _dte;
        }
        public override void OnClick(object sender, EventArgs e)
        {
            string selectedText = GetSelectionText();
            string _using = @"
using Devinux.AutoDI;
";
            var _props = GetEntitiesFromClassDefinitions(selectedText);
            if (_props != null && _props.Count() > 0)
            {
                foreach (var model in _props)
                {
                    var NameSpace = GetActiveProjectNameSpace();
                    // insert command
                    {
                        $@"
    namespace {NameSpace}.{DDDArchitectoriesInformation.Application}.Services.{model.Name}.Commands.Insert;

    public class InsertCommand : ICommand
    {{
        public InsertCommand() {{ }}
    {model.Items.Select(c => $"    {c.TypeName} {c.Name} {{ set; get; }}").ToArray().StringJoin("\n")}
    }}
    ".SaveFile($"{GetActiveProjectFolderPath()}//{DDDArchitectoriesInformation.Application}//Services//{model.Name}//Commands//Insert//InsertCommand.cs");
                        $@"
    namespace {NameSpace}.{DDDArchitectoriesInformation.Application}.Services.{model.Name}.Commands.Insert;

    public class InsertCommandDto 
    {{
        public InsertCommandDto() {{ }}
        public bool Result {{ set; get; }}
    }}
    ".SaveFile($"{GetActiveProjectFolderPath()}//{DDDArchitectoriesInformation.Application}//Services//{model.Name}//Commands//Insert//InsertCommandDto.cs");
                    }
                    // update command
                    {
                        $@"
    namespace {NameSpace}.{DDDArchitectoriesInformation.Application}.Services.{model.Name}.Commands.Update;

    public class UpdateCommand : ICommand
    {{
        public updateCommand() {{ }}
    {model.Items.Select(c => $"    {c.TypeName} {c.Name} {{ set; get; }}").ToArray().StringJoin("\n")}
    }}
    ".SaveFile($"{GetActiveProjectFolderPath()}//{DDDArchitectoriesInformation.Application}//Services//{model.Name}//Commands//Update//UpdateCommand.cs");
                        $@"
    namespace {NameSpace}.{DDDArchitectoriesInformation.Application}.Services.{model.Name}.Commands.Update;

    public class UpdateCommandDto 
    {{
        public UpdateCommandDto() {{ }}
        public bool Result {{ set; get; }}
    }}
    ".SaveFile($"{GetActiveProjectFolderPath()}//{DDDArchitectoriesInformation.Application}//Services//{model.Name}//Commands//Update//UpdateCommandDto.cs");
                    }
                    // delete command
                    {
                        $@"
    namespace {NameSpace}.{DDDArchitectoriesInformation.Application}.Services.{model.Name}.Commands.Delete;

    public class DeleteCommand : ICommand
    {{
        public DeleteCommand() {{ }}
        public int Id {{ set; get; }}
    }}
    ".SaveFile($"{GetActiveProjectFolderPath()}//{DDDArchitectoriesInformation.Application}//Services//{model.Name}//Commands//Delete//DeleteCommand.cs");
                        $@"
    namespace {NameSpace}.{DDDArchitectoriesInformation.Application}.Services.{model.Name}.Commands.Delete;

    public class DeleteCommandDto 
    {{
        public DeleteCommandDto() {{ }}
        public bool Result {{ set; get; }}
    }}
    ".SaveFile($"{GetActiveProjectFolderPath()}//{DDDArchitectoriesInformation.Application}//Services//{model.Name}//Commands//Delete//DeleteCommandDto.cs");
                    }
                    // select query
                    {
                        $@"
    namespace {NameSpace}.{DDDArchitectoriesInformation.Application}.Services.{model.Name}.Queries.Select;

    public class SelectQuery : IQuery
    {{
        public SelectQuery() {{ }}
        public int Id {{ set; get; }}
    
    }}
    ".SaveFile($"{GetActiveProjectFolderPath()}//{DDDArchitectoriesInformation.Application}//Services//{model.Name}//Queries//Select//SelectQuery.cs");
                        $@"
    namespace {NameSpace}.{DDDArchitectoriesInformation.Application}.Services.{model.Name}.Queries.Select;

    public class SelectQueryDto 
    {{
        public SelectQueryDto() {{ }}
{model.Items.Select(c => $"    {c.TypeName} {c.Name} {{ set; get; }}").ToArray().StringJoin("\n")}
    }}
    ".SaveFile($"{GetActiveProjectFolderPath()}//{DDDArchitectoriesInformation.Application}//Services//{model.Name}//Queries//Select//SelectQueryDto.cs");
                    }
                    // List query
                    {
                        $@"
    namespace {NameSpace}.{DDDArchitectoriesInformation.Application}.Services.{model.Name}.Queries.List;

    public class ListQuery : IQuery
    {{
        public SelectQuery() {{ }}
        public string SearchText {{ set; get; }}
    
    }}
    ".SaveFile($"{GetActiveProjectFolderPath()}//{DDDArchitectoriesInformation.Application}//Services//{model.Name}//Queries//List//ListQuery.cs");
                        $@"
    namespace {NameSpace}.{DDDArchitectoriesInformation.Application}.Services.{model.Name}.Queries.List;

    public class ListQueryDto 
    {{
        public SelectQueryDto() {{ }}
{model.Items.Select(c => $"    {c.TypeName} {c.Name} {{ set; get; }}").ToArray().StringJoin("\n")}
    }}
    ".SaveFile($"{GetActiveProjectFolderPath()}//{DDDArchitectoriesInformation.Application}//Services//{model.Name}//Queries//List//ListQueryDto.cs");
                    }
                }
            }
            else
            {
                "selection text is not class".ShowMessage();
            }
        }
    }
    public class CreateModelRefactorCommands : DevinuxCommand
    {
        public override int MenuCommandId { get => PackageIds.CreateModelRefactor; }
        public CreateModelRefactorCommands(DTE _dte)
        {
            dte = _dte;
        }

        
        public override void OnClick(object sender, EventArgs e)
        {
            string solutionPath = System.IO.Path.GetDirectoryName(dte.Solution.FullName);
            var _configPath = solutionPath + "\\.devinux";
            if (!System.IO.Directory.Exists(_configPath)) { System.IO.Directory.CreateDirectory(_configPath); }
            string selectedText = GetSelectionText();
            string _using = @"
using Devinux.AutoDI;
";
            var _props = GetEntitiesFromClassDefinitions(selectedText);
            if (_props != null && _props.Count() > 0)
            {
                Generator g = new Generator(_props.ToArray(),GetActiveProjectNameSpace() , GetActiveProjectFolderPath(), _configPath);
                g.ShowDialog();
            }
            else
            {
                "selection text is not class".ShowMessage();
            }
        }
    }
}