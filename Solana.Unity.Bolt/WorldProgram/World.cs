using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Solana.Unity.Programs.Abstract;
using Solana.Unity.Programs.Utilities;
using Solana.Unity.Programs;
using Solana.Unity.Rpc;
using Solana.Unity.Rpc.Core.Sockets;
using Solana.Unity.Rpc.Types;
using Solana.Unity.Wallet;
using World.Program;
using World.Errors;
using World.Accounts;
using WebSocketSharp;

namespace World
{
    namespace Accounts
    {
        public partial class Entity
        {
            public static ulong ACCOUNT_DISCRIMINATOR => 1751670451238706478UL;
            public static ReadOnlySpan<byte> ACCOUNT_DISCRIMINATOR_BYTES => new byte[]{46, 157, 161, 161, 254, 46, 79, 24};
            public static string ACCOUNT_DISCRIMINATOR_B58 => "8oEQa6zH67R";
            public ulong Id { get; set; }

            public static Entity Deserialize(ReadOnlySpan<byte> _data)
            {
                int offset = 0;
                ulong accountHashValue = _data.GetU64(offset);
                offset += 8;
                if (accountHashValue != ACCOUNT_DISCRIMINATOR)
                {
                    return null;
                }

                Entity result = new Entity();
                result.Id = _data.GetU64(offset);
                offset += 8;
                return result;
            }
        }

        public partial class Registry
        {
            public static ulong ACCOUNT_DISCRIMINATOR => 15779688099924061743UL;
            public static ReadOnlySpan<byte> ACCOUNT_DISCRIMINATOR_BYTES => new byte[]{47, 174, 110, 246, 184, 182, 252, 218};
            public static string ACCOUNT_DISCRIMINATOR_B58 => "8ya1XGY4XBP";
            public ulong Worlds { get; set; }

            public static Registry Deserialize(ReadOnlySpan<byte> _data)
            {
                int offset = 0;
                ulong accountHashValue = _data.GetU64(offset);
                offset += 8;
                if (accountHashValue != ACCOUNT_DISCRIMINATOR)
                {
                    return null;
                }

                Registry result = new Registry();
                result.Worlds = _data.GetU64(offset);
                offset += 8;
                return result;
            }
        }

        public partial class World
        {
            public static ulong ACCOUNT_DISCRIMINATOR => 8978805993381703057UL;
            public static ReadOnlySpan<byte> ACCOUNT_DISCRIMINATOR_BYTES => new byte[]{145, 45, 170, 174, 122, 32, 155, 124};
            public static string ACCOUNT_DISCRIMINATOR_B58 => "RHQudtaQtu1";
            public ulong Id { get; set; }

            public ulong Entities { get; set; }

            public static World Deserialize(ReadOnlySpan<byte> _data)
            {
                int offset = 0;
                ulong accountHashValue = _data.GetU64(offset);
                offset += 8;
                if (accountHashValue != ACCOUNT_DISCRIMINATOR)
                {
                    return null;
                }

                World result = new World();
                result.Id = _data.GetU64(offset);
                offset += 8;
                result.Entities = _data.GetU64(offset);
                offset += 8;
                return result;
            }
        }
    }

    namespace Errors
    {
        public enum WorldErrorKind : uint
        {
            InvalidAuthority = 6000U
        }
    }

    public partial class WorldClient : TransactionalBaseClient<WorldErrorKind>
    {
        public static PublicKey ProgramID = new PublicKey(WorldProgram.ID);
        
        public WorldClient(IRpcClient rpcClient, IStreamingRpcClient streamingRpcClient)
            : base(rpcClient, streamingRpcClient, new PublicKey(WorldProgram.ID))
        {
        }
        
        public WorldClient(IRpcClient rpcClient, IStreamingRpcClient streamingRpcClient, PublicKey programId) : base(rpcClient, streamingRpcClient, programId)
        {
        }

        public async Task<Solana.Unity.Programs.Models.ProgramAccountsResultWrapper<List<Entity>>> GetEntitysAsync(string programAddress = WorldProgram.ID, Commitment commitment = Commitment.Finalized)
        {
            var list = new List<Solana.Unity.Rpc.Models.MemCmp>{new Solana.Unity.Rpc.Models.MemCmp{Bytes = Entity.ACCOUNT_DISCRIMINATOR_B58, Offset = 0}};
            var res = await RpcClient.GetProgramAccountsAsync(programAddress, commitment, memCmpList: list);
            if (!res.WasSuccessful || !(res.Result?.Count > 0))
                return new Solana.Unity.Programs.Models.ProgramAccountsResultWrapper<List<Entity>>(res);
            List<Entity> resultingAccounts = new List<Entity>(res.Result.Count);
            resultingAccounts.AddRange(res.Result.Select(result => Entity.Deserialize(Convert.FromBase64String(result.Account.Data[0]))));
            return new Solana.Unity.Programs.Models.ProgramAccountsResultWrapper<List<Entity>>(res, resultingAccounts);
        }

        public async Task<Solana.Unity.Programs.Models.ProgramAccountsResultWrapper<List<Registry>>> GetRegistrysAsync(string programAddress = WorldProgram.ID, Commitment commitment = Commitment.Finalized)
        {
            var list = new List<Solana.Unity.Rpc.Models.MemCmp>{new Solana.Unity.Rpc.Models.MemCmp{Bytes = Registry.ACCOUNT_DISCRIMINATOR_B58, Offset = 0}};
            var res = await RpcClient.GetProgramAccountsAsync(programAddress, commitment, memCmpList: list);
            if (!res.WasSuccessful || !(res.Result?.Count > 0))
                return new Solana.Unity.Programs.Models.ProgramAccountsResultWrapper<List<Registry>>(res);
            List<Registry> resultingAccounts = new List<Registry>(res.Result.Count);
            resultingAccounts.AddRange(res.Result.Select(result => Registry.Deserialize(Convert.FromBase64String(result.Account.Data[0]))));
            return new Solana.Unity.Programs.Models.ProgramAccountsResultWrapper<List<Registry>>(res, resultingAccounts);
        }

        public async Task<Solana.Unity.Programs.Models.ProgramAccountsResultWrapper<List<World.Accounts.World>>> GetWorldsAsync(string programAddress = WorldProgram.ID, Commitment commitment = Commitment.Finalized)
        {
            var list = new List<Solana.Unity.Rpc.Models.MemCmp>{new Solana.Unity.Rpc.Models.MemCmp{Bytes = World.Accounts.World.ACCOUNT_DISCRIMINATOR_B58, Offset = 0}};
            var res = await RpcClient.GetProgramAccountsAsync(programAddress, commitment, memCmpList: list);
            if (!res.WasSuccessful || !(res.Result?.Count > 0))
                return new Solana.Unity.Programs.Models.ProgramAccountsResultWrapper<List<World.Accounts.World>>(res);
            List<World.Accounts.World> resultingAccounts = new List<World.Accounts.World>(res.Result.Count);
            resultingAccounts.AddRange(res.Result.Select(result => World.Accounts.World.Deserialize(Convert.FromBase64String(result.Account.Data[0]))));
            return new Solana.Unity.Programs.Models.ProgramAccountsResultWrapper<List<World.Accounts.World>>(res, resultingAccounts);
        }

        public async Task<Solana.Unity.Programs.Models.AccountResultWrapper<Entity>> GetEntityAsync(string accountAddress, Commitment commitment = Commitment.Finalized)
        {
            var res = await RpcClient.GetAccountInfoAsync(accountAddress, commitment);
            if (!res.WasSuccessful)
                return new Solana.Unity.Programs.Models.AccountResultWrapper<Entity>(res);
            var resultingAccount = Entity.Deserialize(Convert.FromBase64String(res.Result.Value.Data[0]));
            return new Solana.Unity.Programs.Models.AccountResultWrapper<Entity>(res, resultingAccount);
        }

        public async Task<Solana.Unity.Programs.Models.AccountResultWrapper<Registry>> GetRegistryAsync(string accountAddress, Commitment commitment = Commitment.Finalized)
        {
            var res = await RpcClient.GetAccountInfoAsync(accountAddress, commitment);
            if (!res.WasSuccessful)
                return new Solana.Unity.Programs.Models.AccountResultWrapper<Registry>(res);
            var resultingAccount = Registry.Deserialize(Convert.FromBase64String(res.Result.Value.Data[0]));
            return new Solana.Unity.Programs.Models.AccountResultWrapper<Registry>(res, resultingAccount);
        }

        public async Task<Solana.Unity.Programs.Models.AccountResultWrapper<World.Accounts.World>> GetWorldAsync(string accountAddress, Commitment commitment = Commitment.Finalized)
        {
            var res = await RpcClient.GetAccountInfoAsync(accountAddress, commitment);
            if (!res.WasSuccessful || res.Result.Value == null)
                return new Solana.Unity.Programs.Models.AccountResultWrapper<World.Accounts.World>(res);
            var resultingAccount = World.Accounts.World.Deserialize(Convert.FromBase64String(res.Result.Value.Data[0]));
            return new Solana.Unity.Programs.Models.AccountResultWrapper<World.Accounts.World>(res, resultingAccount);
        }

        public async Task<SubscriptionState> SubscribeEntityAsync(string accountAddress, Action<SubscriptionState, Solana.Unity.Rpc.Messages.ResponseValue<Solana.Unity.Rpc.Models.AccountInfo>, Entity> callback, Commitment commitment = Commitment.Finalized)
        {
            SubscriptionState res = await StreamingRpcClient.SubscribeAccountInfoAsync(accountAddress, (s, e) =>
            {
                Entity parsingResult = null;
                if (e.Value?.Data?.Count > 0)
                    parsingResult = Entity.Deserialize(Convert.FromBase64String(e.Value.Data[0]));
                callback(s, e, parsingResult);
            }, commitment);
            return res;
        }

        public async Task<SubscriptionState> SubscribeRegistryAsync(string accountAddress, Action<SubscriptionState, Solana.Unity.Rpc.Messages.ResponseValue<Solana.Unity.Rpc.Models.AccountInfo>, Registry> callback, Commitment commitment = Commitment.Finalized)
        {
            SubscriptionState res = await StreamingRpcClient.SubscribeAccountInfoAsync(accountAddress, (s, e) =>
            {
                Registry parsingResult = null;
                if (e.Value?.Data?.Count > 0)
                    parsingResult = Registry.Deserialize(Convert.FromBase64String(e.Value.Data[0]));
                callback(s, e, parsingResult);
            }, commitment);
            return res;
        }

        public async Task<SubscriptionState> SubscribeWorldAsync(string accountAddress, Action<SubscriptionState, Solana.Unity.Rpc.Messages.ResponseValue<Solana.Unity.Rpc.Models.AccountInfo>, World.Accounts.World> callback, Commitment commitment = Commitment.Finalized)
        {
            SubscriptionState res = await StreamingRpcClient.SubscribeAccountInfoAsync(accountAddress, (s, e) =>
            {
                World.Accounts.World parsingResult = null;
                if (e.Value?.Data?.Count > 0)
                    parsingResult = World.Accounts.World.Deserialize(Convert.FromBase64String(e.Value.Data[0]));
                callback(s, e, parsingResult);
            }, commitment);
            return res;
        }

        protected override Dictionary<uint, ProgramError<WorldErrorKind>> BuildErrorsDictionary()
        {
            return new Dictionary<uint, ProgramError<WorldErrorKind>>{{6000U, new ProgramError<WorldErrorKind>(WorldErrorKind.InvalidAuthority, "Invalid authority for instruction")}, };
        }
    }

    namespace Program
    {
        public class InitializeRegistryAccounts
        {
            public PublicKey Registry { get; set; }

            public PublicKey Payer { get; set; }

            public PublicKey SystemProgram { get; set; }
        }

        public class InitializeNewWorldAccounts
        {
            public PublicKey Payer { get; set; }

            public PublicKey World { get; set; }

            public PublicKey Registry { get; set; }

            public PublicKey SystemProgram { get; set; }
        }

        public class AddEntityAccounts
        {
            public PublicKey Payer { get; set; }

            public PublicKey Entity { get; set; }

            public PublicKey World { get; set; }

            public PublicKey SystemProgram { get; set; }
        }

        public class InitializeComponentAccounts
        {
            public PublicKey Payer { get; set; }

            public PublicKey Data { get; set; }

            public PublicKey Entity { get; set; }

            public PublicKey ComponentProgram { get; set; }

            public PublicKey Authority { get; set; } = new(WorldProgram.ID);

            public PublicKey InstructionSysvarAccount { get; set; } = SysVars.InstructionAccount;

            public PublicKey SystemProgram { get; set; }
        }

        public class ApplyAccounts
        {
            public PublicKey ComponentProgram { get; set; }

            public PublicKey BoltSystem { get; set; }

            public PublicKey BoltComponent { get; set; }

            public PublicKey Authority { get; set; }

            public PublicKey InstructionSysvarAccount { get; set; }
        }

        public class Apply2Accounts
        {
            public PublicKey BoltSystem { get; set; }

            public PublicKey ComponentProgram1 { get; set; }

            public PublicKey BoltComponent1 { get; set; }

            public PublicKey ComponentProgram2 { get; set; }

            public PublicKey BoltComponent2 { get; set; }

            public PublicKey Authority { get; set; }

            public PublicKey InstructionSysvarAccount { get; set; }
        }

        public class Apply3Accounts
        {
            public PublicKey BoltSystem { get; set; }

            public PublicKey ComponentProgram1 { get; set; }

            public PublicKey BoltComponent1 { get; set; }

            public PublicKey ComponentProgram2 { get; set; }

            public PublicKey BoltComponent2 { get; set; }

            public PublicKey ComponentProgram3 { get; set; }

            public PublicKey BoltComponent3 { get; set; }

            public PublicKey Authority { get; set; }

            public PublicKey InstructionSysvarAccount { get; set; }
        }

        public class Apply4Accounts
        {
            public PublicKey BoltSystem { get; set; }

            public PublicKey ComponentProgram1 { get; set; }

            public PublicKey BoltComponent1 { get; set; }

            public PublicKey ComponentProgram2 { get; set; }

            public PublicKey BoltComponent2 { get; set; }

            public PublicKey ComponentProgram3 { get; set; }

            public PublicKey BoltComponent3 { get; set; }

            public PublicKey ComponentProgram4 { get; set; }

            public PublicKey BoltComponent4 { get; set; }

            public PublicKey Authority { get; set; }

            public PublicKey InstructionSysvarAccount { get; set; }
        }

        public class Apply5Accounts
        {
            public PublicKey BoltSystem { get; set; }

            public PublicKey ComponentProgram1 { get; set; }

            public PublicKey BoltComponent1 { get; set; }

            public PublicKey ComponentProgram2 { get; set; }

            public PublicKey BoltComponent2 { get; set; }

            public PublicKey ComponentProgram3 { get; set; }

            public PublicKey BoltComponent3 { get; set; }

            public PublicKey ComponentProgram4 { get; set; }

            public PublicKey BoltComponent4 { get; set; }

            public PublicKey ComponentProgram5 { get; set; }

            public PublicKey BoltComponent5 { get; set; }

            public PublicKey Authority { get; set; }

            public PublicKey InstructionSysvarAccount { get; set; }
        }

        public static class WorldProgram
        {
            public const string ID = "WorLD15A7CrDwLcLy4fRqtaTb9fbd8o8iqiEMUDse2n";
            public static Solana.Unity.Rpc.Models.TransactionInstruction InitializeRegistry(InitializeRegistryAccounts accounts, PublicKey programId = null)
            {
                programId ??= new(ID);
                List<Solana.Unity.Rpc.Models.AccountMeta> keys = new()
                {Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.Registry, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.Payer, true), Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.SystemProgram, false)};
                byte[] _data = new byte[1200];
                int offset = 0;
                _data.WriteU64(4321548737212364221UL, offset);
                offset += 8;
                byte[] resultData = new byte[offset];
                Array.Copy(_data, resultData, offset);
                return new Solana.Unity.Rpc.Models.TransactionInstruction{Keys = keys, ProgramId = programId.KeyBytes, Data = resultData};
            }

            public static Solana.Unity.Rpc.Models.TransactionInstruction InitializeNewWorld(InitializeNewWorldAccounts accounts, PublicKey programId = null)
            {
                programId ??= new(ID);
                List<Solana.Unity.Rpc.Models.AccountMeta> keys = new()
                {Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.Payer, true), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.World, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.Registry, false), Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.SystemProgram, false)};
                byte[] _data = new byte[1200];
                int offset = 0;
                _data.WriteU64(7118163274173538327UL, offset);
                offset += 8;
                byte[] resultData = new byte[offset];
                Array.Copy(_data, resultData, offset);
                return new Solana.Unity.Rpc.Models.TransactionInstruction{Keys = keys, ProgramId = programId.KeyBytes, Data = resultData};
            }

            public static Solana.Unity.Rpc.Models.TransactionInstruction AddEntity(AddEntityAccounts accounts, PublicKey programId = null)
            {
                programId ??= new(ID);
                return AddEntity(accounts, "", programId);
            }

            public static Solana.Unity.Rpc.Models.TransactionInstruction AddEntity(AddEntityAccounts accounts, string extraSeed, PublicKey programId = null)
            {
                programId ??= new(ID);
                List<Solana.Unity.Rpc.Models.AccountMeta> keys = new()
                {
                    Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.Payer, true), 
                    Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.Entity, false), 
                    Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.World, false), 
                    Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.SystemProgram, false)
                };

                byte[] _data = new byte[1200];
                int offset = 0;
                _data.WriteU64(4121062988444201379UL, offset);
                offset += 8;
                if (!extraSeed.IsNullOrEmpty())
                {
                    _data.WriteU8(1, offset);
                    offset += 1;
                    offset += _data.WriteBorshString(extraSeed, offset);
                }
                else
                {
                    _data.WriteU8(0, offset);
                    // Offset is 2 in https://github.com/GabrielePicco/chainy-unity/blob/main/Assets/Bolt/WorldInteraction.cs#L388
                    // Possibly correct?
                    offset += 2; 
                }

                byte[] resultData = new byte[offset];
                Array.Copy(_data, resultData, offset);
                return new Solana.Unity.Rpc.Models.TransactionInstruction{Keys = keys, ProgramId = programId.KeyBytes, Data = resultData};
            }

            public static Solana.Unity.Rpc.Models.TransactionInstruction InitializeComponent(InitializeComponentAccounts accounts, PublicKey programId = null)
            {
                programId ??= new(ID);
                List<Solana.Unity.Rpc.Models.AccountMeta> keys = new()
                {
                    Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.Payer, true), 
                    Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.Data, false), 
                    Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.Entity, false), 
                    Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.ComponentProgram, false), 
                    Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.Authority, false), 
                    Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.InstructionSysvarAccount, false), 
                    Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.SystemProgram, false)
                };
                byte[] _data = new byte[1200];
                int offset = 0;
                _data.WriteU64(2179155133888827172UL, offset);
                offset += 8;
                byte[] resultData = new byte[offset];
                Array.Copy(_data, resultData, offset);
                return new Solana.Unity.Rpc.Models.TransactionInstruction{Keys = keys, ProgramId = programId.KeyBytes, Data = resultData};
            }

            public static PublicKey FindComponentPda(
                PublicKey componentProgramId, 
                PublicKey entity, 
                string componentId = "")
            {
                PublicKey.TryFindProgramAddress(new[]
                {
                    Encoding.UTF8.GetBytes(componentId), entity.KeyBytes
                }, componentProgramId, out var pda, out _);
                return pda;
            }

            /// <summary>
            /// Convienence bundle for defining an entity and the associated components.
            /// </summary>
            public class EntityType{
                public PublicKey[] Components { get; set; }
                public string[] Seeds { get; set; }
                public PublicKey Entity { get; set; }

                public EntityType(PublicKey entity, PublicKey[] componentsIds)
                {
                    Components = componentsIds;
                    Seeds = new string[Components.Length];
                    Entity = entity;
                    Array.Fill(Seeds, "");
                }

                public EntityType(PublicKey entity, PublicKey[] componentsIds, string[] seeds)
                {
                    Components = componentsIds;
                    Seeds = seeds;
                    Entity = entity;
                    if (Seeds.Length != Components.Length)
                    {
                        throw new ArgumentException("Seeds must be the same length as components");
                    }
                }

                public int ComponentsLength()
                {
                    return Components.Length;
                }

                public PublicKey[] GetComponentsIds()
                {
                    return Components;
                }
                public PublicKey[] GetComponentsPdas()
                {
                    PublicKey[] pdas = new PublicKey[Components.Length];
                    for (int i = 0; i < Components.Length; i++)
                    {
                        pdas[i] = FindComponentPda(Components[i], Entity, Seeds[i]);
                    }
                    return pdas;
                }
            }


            // The current World program rely on a "trick", where the Apply function is overloaded with multiple Apply* functions, each with a different number of components.
            // This is done since Solana require the same number of accounts for each instruction, and the number of components can vary.
            // The Apply function is then called with the correct Apply* function based on the number of components.
            // This is a workaround, and will be replaced in a next Bolt release (as Anchor now support custom Discriminator).
            // -
            // The ApplySystem Gives a common API that we can change in the future. 
            public static Solana.Unity.Rpc.Models.TransactionInstruction ApplySystem(
                PublicKey system, 
                EntityType[] systemInput, 
                byte[] args, 
                PublicKey authority, 
                PublicKey programId = null)
            {
                programId ??= new(ID);
                int numComponents = 0;
                foreach (var entity in systemInput)
                {
                    numComponents += entity.ComponentsLength();
                }

                if (numComponents == 0)
                {
                    throw new ArgumentException("No components provided");
                }

                // Create lists to hold all IDs and PDAs if needed
                List<PublicKey> componentIds = new List<PublicKey>();
                List<PublicKey> componentPdas = new List<PublicKey>();

                foreach (var entity in systemInput)
                {
                    componentIds.AddRange(entity.GetComponentsIds());
                    componentPdas.AddRange(entity.GetComponentsPdas());
                }

                // The logic below assumes there are different handling methods for different numbers of components
                switch (numComponents)
                {
                    case 1:
                        var applyAccounts = new ApplyAccounts
                        {
                            BoltSystem = system,
                            ComponentProgram = componentIds[0],
                            BoltComponent = componentPdas[0],
                            Authority = authority,
                            InstructionSysvarAccount = SysVars.InstructionAccount,
                        };
                        return Apply1(applyAccounts, args, programId);

                    case 2:
                        var apply2Accounts = new Apply2Accounts
                        {
                            BoltSystem = system,
                            ComponentProgram1 = componentIds[0],
                            ComponentProgram2 = componentIds[1],
                            BoltComponent1 = componentPdas[0],
                            BoltComponent2 = componentPdas[1],
                            Authority = authority,
                            InstructionSysvarAccount = SysVars.InstructionAccount,
                        };
                        return Apply2(apply2Accounts, args, programId);

                    case 3:
                        var apply3Accounts = new Apply3Accounts
                        {
                            BoltSystem = system,
                            ComponentProgram1 = componentIds[0],
                            ComponentProgram2 = componentIds[1],
                            ComponentProgram3 = componentIds[2],
                            BoltComponent1 = componentPdas[0],
                            BoltComponent2 = componentPdas[1],
                            BoltComponent3 = componentPdas[2],
                            Authority = authority,
                            InstructionSysvarAccount = SysVars.InstructionAccount,
                        };
                        return Apply3(apply3Accounts, args, programId);

                    case 4:
                        var apply4Accounts = new Apply4Accounts
                        {
                            BoltSystem = system,
                            ComponentProgram1 = componentIds[0],
                            ComponentProgram2 = componentIds[1],
                            ComponentProgram3 = componentIds[2],
                            ComponentProgram4 = componentIds[3],
                            BoltComponent1 = componentPdas[0],
                            BoltComponent2 = componentPdas[1],
                            BoltComponent3 = componentPdas[2],
                            BoltComponent4 = componentPdas[3],
                            Authority = authority,
                            InstructionSysvarAccount = SysVars.InstructionAccount,
                        };
                        return Apply4(apply4Accounts, args, programId);

                    case 5:
                        var apply5Accounts = new Apply5Accounts
                        {
                            BoltSystem = system,
                            ComponentProgram1 = componentIds[0],
                            ComponentProgram2 = componentIds[1],
                            ComponentProgram3 = componentIds[2],
                            ComponentProgram4 = componentIds[3],
                            ComponentProgram5 = componentIds[4],
                            BoltComponent1 = componentPdas[0],
                            BoltComponent2 = componentPdas[1],
                            BoltComponent3 = componentPdas[2],
                            BoltComponent4 = componentPdas[3],
                            BoltComponent5 = componentPdas[4],
                            Authority = authority,
                            InstructionSysvarAccount = SysVars.InstructionAccount,
                        };
                        return Apply5(apply5Accounts, args, programId);

                    default:
                        throw new ArgumentException("Unsupported number of components");
                }
            }

            private static Solana.Unity.Rpc.Models.TransactionInstruction Apply1(ApplyAccounts accounts, byte[] args, PublicKey programId)
            {
                List<Solana.Unity.Rpc.Models.AccountMeta> keys = new()
                {Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.ComponentProgram, false), Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.BoltSystem, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.BoltComponent, false), Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.Authority, false), Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.InstructionSysvarAccount, false)};
                byte[] _data = new byte[1200];
                int offset = 0;
                _data.WriteU64(16258613031726085112UL, offset);
                offset += 8;
                _data.WriteS32(args.Length, offset);
                offset += 4;
                _data.WriteSpan(args, offset);
                offset += args.Length;
                byte[] resultData = new byte[offset];
                Array.Copy(_data, resultData, offset);
                return new Solana.Unity.Rpc.Models.TransactionInstruction{Keys = keys, ProgramId = programId.KeyBytes, Data = resultData};
            }

            private static Solana.Unity.Rpc.Models.TransactionInstruction Apply2(Apply2Accounts accounts, byte[] args, PublicKey programId)
            {
                List<Solana.Unity.Rpc.Models.AccountMeta> keys = new()
                {Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.BoltSystem, false), Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.ComponentProgram1, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.BoltComponent1, false), Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.ComponentProgram2, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.BoltComponent2, false), Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.Authority, false), Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.InstructionSysvarAccount, false)};
                byte[] _data = new byte[1200];
                int offset = 0;
                _data.WriteU64(5318926663469506680UL, offset);
                offset += 8;
                _data.WriteS32(args.Length, offset);
                offset += 4;
                _data.WriteSpan(args, offset);
                offset += args.Length;
                byte[] resultData = new byte[offset];
                Array.Copy(_data, resultData, offset);
                return new Solana.Unity.Rpc.Models.TransactionInstruction{Keys = keys, ProgramId = programId.KeyBytes, Data = resultData};
            }

            private static Solana.Unity.Rpc.Models.TransactionInstruction Apply3(Apply3Accounts accounts, byte[] args, PublicKey programId)
            {
                List<Solana.Unity.Rpc.Models.AccountMeta> keys = new()
                {Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.BoltSystem, false), Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.ComponentProgram1, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.BoltComponent1, false), Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.ComponentProgram2, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.BoltComponent2, false), Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.ComponentProgram3, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.BoltComponent3, false), Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.Authority, false), Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.InstructionSysvarAccount, false)};
                byte[] _data = new byte[1200];
                int offset = 0;
                _data.WriteU64(15954428204692902654UL, offset);
                offset += 8;
                _data.WriteS32(args.Length, offset);
                offset += 4;
                _data.WriteSpan(args, offset);
                offset += args.Length;
                byte[] resultData = new byte[offset];
                Array.Copy(_data, resultData, offset);
                return new Solana.Unity.Rpc.Models.TransactionInstruction{Keys = keys, ProgramId = programId.KeyBytes, Data = resultData};
            }

            private static Solana.Unity.Rpc.Models.TransactionInstruction Apply4(Apply4Accounts accounts, byte[] args, PublicKey programId)
            {
                List<Solana.Unity.Rpc.Models.AccountMeta> keys = new()
                {Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.BoltSystem, false), Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.ComponentProgram1, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.BoltComponent1, false), Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.ComponentProgram2, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.BoltComponent2, false), Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.ComponentProgram3, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.BoltComponent3, false), Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.ComponentProgram4, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.BoltComponent4, false), Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.Authority, false), Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.InstructionSysvarAccount, false)};
                byte[] _data = new byte[1200];
                int offset = 0;
                _data.WriteU64(7858434987745896671UL, offset);
                offset += 8;
                _data.WriteS32(args.Length, offset);
                offset += 4;
                _data.WriteSpan(args, offset);
                offset += args.Length;
                byte[] resultData = new byte[offset];
                Array.Copy(_data, resultData, offset);
                return new Solana.Unity.Rpc.Models.TransactionInstruction{Keys = keys, ProgramId = programId.KeyBytes, Data = resultData};
            }

            private static Solana.Unity.Rpc.Models.TransactionInstruction Apply5(Apply5Accounts accounts, byte[] args, PublicKey programId)
            {
                List<Solana.Unity.Rpc.Models.AccountMeta> keys = new()
                {Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.BoltSystem, false), Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.ComponentProgram1, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.BoltComponent1, false), Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.ComponentProgram2, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.BoltComponent2, false), Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.ComponentProgram3, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.BoltComponent3, false), Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.ComponentProgram4, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.BoltComponent4, false), Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.ComponentProgram5, false), Solana.Unity.Rpc.Models.AccountMeta.Writable(accounts.BoltComponent5, false), Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.Authority, false), Solana.Unity.Rpc.Models.AccountMeta.ReadOnly(accounts.InstructionSysvarAccount, false)};
                byte[] _data = new byte[1200];
                int offset = 0;
                _data.WriteU64(11048583913794872390UL, offset);
                offset += 8;
                _data.WriteS32(args.Length, offset);
                offset += 4;
                _data.WriteSpan(args, offset);
                offset += args.Length;
                byte[] resultData = new byte[offset];
                Array.Copy(_data, resultData, offset);
                return new Solana.Unity.Rpc.Models.TransactionInstruction{Keys = keys, ProgramId = programId.KeyBytes, Data = resultData};
            }
        }
    }
}