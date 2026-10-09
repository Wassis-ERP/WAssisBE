-- ONLY for a disposable local database after migrations. Never run against HML/PRD.
INSERT INTO erp.tenants (id,ativo) VALUES ('22222222-2222-2222-2222-222222222222',true);
INSERT INTO erp.filiais (id,tenant_id,nome,ativo) VALUES
('44444444-4444-4444-4444-444444444444','22222222-2222-2222-2222-222222222222','Corretora de teste local',true);
INSERT INTO erp.pipelines (id,tenant_id,filial_id,nome,entidade_tipo,ativo) VALUES
('55555555-5555-5555-5555-555555555555','22222222-2222-2222-2222-222222222222','44444444-4444-4444-4444-444444444444','Comercial','oportunidade',true);
INSERT INTO erp.pipeline_stages (id,pipeline_id,nome,tipo_stage,ordem,cor,ativo,finaliza_com_sucesso,finaliza_com_perda) VALUES
('66666666-6666-6666-6666-666666666666','55555555-5555-5555-5555-555555555555','Em aberto','ABERTO',1,'#0099e5',true,true,true),
('66666666-6666-6666-6666-666666666667','55555555-5555-5555-5555-555555555555','Ganha','GANHO',2,'#0099e5',true,true,true),
('66666666-6666-6666-6666-666666666668','55555555-5555-5555-5555-555555555555','Perdida','PERDIDO',3,'#0099e5',true,true,true);
INSERT INTO erp.ramos (id,tenant_id,nome,ativo,risk_type,grupo_operacional,forma_calculo,is_monthly,renovavel,permite_endosso,exige_item,exige_coberturas) VALUES
('77777777-7777-7777-7777-777777777777','22222222-2222-2222-2222-222222222222','Auto teste local',true,'VEICULO','Auto e Frota','AUTO',false,true,true,true,true);
INSERT INTO erp.profiles (id,tenant_id,nome_completo,email,status,ativo,convite_status) VALUES
('11111111-1111-1111-1111-111111111111','22222222-2222-2222-2222-222222222222','Usuário sintético','user0@example.invalid','ATIVO',true,'ACEITO');
INSERT INTO erp.perfis (id,tenant_id,nome,sistema,ativo) VALUES
('88888888-8888-8888-8888-888888888888','22222222-2222-2222-2222-222222222222','Perfil sintético',false,true);
INSERT INTO erp.profile_filiais (id,profile_id,filial_id,perfil_id,principal,ativo) VALUES
(gen_random_uuid(),'11111111-1111-1111-1111-111111111111','44444444-4444-4444-4444-444444444444','88888888-8888-8888-8888-888888888888',true,true);
INSERT INTO erp.role_permissions (id,perfil_id,modulo,escopo,can_read,can_create,can_update,can_delete,can_export,can_manage)
SELECT gen_random_uuid(),'88888888-8888-8888-8888-888888888888',modulo,escopo,true,true,true,false,false,false
FROM (VALUES ('segurados','CORRETORA'),('comercial','CORRETORA'),('configuracoes','GRUPO')) AS p(modulo,escopo);
