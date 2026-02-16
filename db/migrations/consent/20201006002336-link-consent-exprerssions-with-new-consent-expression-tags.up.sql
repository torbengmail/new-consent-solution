INSERT INTO consent.consent_expression_tag_list (consent_expression_id, consent_expression_tag_id)
select e.id as consent_expression_id,
    (select id from consent.consent_expression_tag where name = t.name and owner_id = o.id) as tag_id
from consent.consent c
         join data_inventory.owner o on o.id = c.owner_id
         join consent.consent_expression e on c.id = e.consent_id
         join consent.consent_expression_tag_list l on e.id = l.consent_expression_id
         join consent.consent_expression_tag t on l.consent_expression_tag_id = t.id
order by t.name, o.id;