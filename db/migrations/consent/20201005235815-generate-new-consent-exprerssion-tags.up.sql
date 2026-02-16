ALTER SEQUENCE consent.consent_expression_tag_id_seq RESTART WITH 1000;
--;;
INSERT INTO consent.consent_expression_tag (id, name, owner_id)
select nextval('consent.consent_expression_tag_id_seq') as tag_id, t.name as tag_name, o.id as owner_id
from consent.consent c
         join data_inventory.owner o on o.id = c.owner_id
         join consent.consent_expression e on c.id = e.consent_id
         join consent.consent_expression_tag_list l on e.id = l.consent_expression_id
         join consent.consent_expression_tag t on l.consent_expression_tag_id = t.id
group by t.name, o.id, t.id
order by tag_name, owner_id;