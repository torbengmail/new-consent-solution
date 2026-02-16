--;; Table: data_inventory.agreement_state
CREATE TABLE IF NOT EXISTS data_inventory.agreement_state (
    id INTEGER,
    name TEXT NOT NULL,
    CONSTRAINT agreement_state_pkey PRIMARY KEY (id),
    CONSTRAINT agreement_state_name_key UNIQUE (name)
);

--;; Table: data_inventory.agreement_type
CREATE TABLE IF NOT EXISTS data_inventory.agreement_type (
    id INTEGER,
    name TEXT NOT NULL,
    CONSTRAINT agreement_type_pkey PRIMARY KEY (id),
    CONSTRAINT agreement_type_name_key UNIQUE (name)
);

--;; Table: data_inventory.agreement
CREATE TABLE IF NOT EXISTS data_inventory.agreement (
    id SERIAL,
    name TEXT NOT NULL,
    agreement_state_id INTEGER,
    agreement_date TIMESTAMP WITHOUT TIME ZONE,
    link TEXT,
    agreement_type_id INTEGER NOT NULL,
    has_scc BOOLEAN,
    remark TEXT,
    CONSTRAINT agreement_pkey PRIMARY KEY (id),
    CONSTRAINT agreement_agreement_state_id_fkey FOREIGN KEY (agreement_state_id) REFERENCES data_inventory.agreement_state (id),
    CONSTRAINT agreement_agreement_type_id_fkey FOREIGN KEY (agreement_type_id) REFERENCES data_inventory.agreement_type (id)
);

--;; Table: data_inventory.owner_type
CREATE TABLE IF NOT EXISTS data_inventory.owner_type (
    id INTEGER,
    name TEXT NOT NULL,
    CONSTRAINT owner_type_pkey PRIMARY KEY (id),
    CONSTRAINT owner_type_name_key UNIQUE (name)
);

--;; Table: data_inventory.country
CREATE TABLE IF NOT EXISTS data_inventory.country (
    id INTEGER,
    name TEXT NOT NULL,
    has_gdpr BOOLEAN,
    has_adequacy_decision BOOLEAN,
    CONSTRAINT country_pkey PRIMARY KEY (id),
    CONSTRAINT country_name_key UNIQUE (name)
);

--;; Table: data_inventory.owner
CREATE TABLE IF NOT EXISTS data_inventory.owner (
    id SERIAL,
    name TEXT NOT NULL,
    has_bcr BOOLEAN,
    registration_country_id INTEGER NOT NULL,
    owner_type_id INTEGER NOT NULL,
    CONSTRAINT owner_pkey PRIMARY KEY (id),
    CONSTRAINT owner_owner_type_id_fkey FOREIGN KEY (owner_type_id) REFERENCES data_inventory.owner_type (id),
    CONSTRAINT owner_registration_country_id_fkey FOREIGN KEY (registration_country_id) REFERENCES data_inventory.country (id)
);

--;; Table: data_inventory.owner_establishment
CREATE TABLE IF NOT EXISTS data_inventory.owner_establishment (
    country_id INTEGER NOT NULL,
    owner_id INTEGER NOT NULL,
    CONSTRAINT owner_establishment_pkey PRIMARY KEY (country_id, owner_id),
    CONSTRAINT owner_establishment_country_id_fkey FOREIGN KEY (country_id) REFERENCES data_inventory.country (id),
    CONSTRAINT owner_establishment_owner_id_fkey FOREIGN KEY (owner_id) REFERENCES data_inventory.owner (id)
);

--;; Table: data_inventory.owner_role
CREATE TABLE IF NOT EXISTS data_inventory.owner_role (
    id INTEGER,
    name TEXT NOT NULL,
    CONSTRAINT owner_role_pkey PRIMARY KEY (id),
    CONSTRAINT owner_role_role_key UNIQUE (name)
);

--;; Table: data_inventory.owner_agreement
CREATE TABLE IF NOT EXISTS data_inventory.owner_agreement (
    id SERIAL,
    owner_id INTEGER NOT NULL,
    agreement_id INTEGER NOT NULL,
    owner_role_id INTEGER NOT NULL,
    CONSTRAINT owner_agreement_pkey PRIMARY KEY (id),
    CONSTRAINT owner_agreement_constraint UNIQUE (agreement_id, owner_id),
    CONSTRAINT owner_agreement_agreement_id_fkey FOREIGN KEY (agreement_id) REFERENCES data_inventory.agreement (id),
    CONSTRAINT owner_agreement_owner_id_fkey FOREIGN KEY (owner_id) REFERENCES data_inventory.owner (id),
    CONSTRAINT owner_agreement_owner_role_id_fkey FOREIGN KEY (owner_role_id) REFERENCES data_inventory.owner_role (id)
);

--;; Table: data_inventory.product_group
CREATE TABLE IF NOT EXISTS data_inventory.product_group (
    id INTEGER,
    name TEXT NOT NULL,
    CONSTRAINT product_group_pkey PRIMARY KEY (id),
    CONSTRAINT product_group_name_key UNIQUE (name)
);

--;; Table: data_inventory.product
CREATE TABLE IF NOT EXISTS data_inventory.product (
    id SERIAL,
    name TEXT NOT NULL,
    product_group_id INTEGER NOT NULL,
    description TEXT,
    owner_id INTEGER NOT NULL,
    is_visible BOOLEAN NOT NULL DEFAULT TRUE,
    CONSTRAINT product_pkey PRIMARY KEY (id),
    CONSTRAINT product_name_key UNIQUE (name),
    CONSTRAINT product_owner_id_fkey FOREIGN KEY (owner_id) REFERENCES data_inventory.owner (id),
    CONSTRAINT product_product_group_id_fkey FOREIGN KEY (product_group_id) REFERENCES data_inventory.product_group (id)
);

--;; Table: data_inventory.service
CREATE TABLE IF NOT EXISTS data_inventory.service (
    id SERIAL,
    name TEXT NOT NULL,
    owner_id INTEGER NOT NULL,
    default_role_id INTEGER,
    rank INTEGER NOT NULL DEFAULT 65535,
    product_id INTEGER,
    CONSTRAINT service_pkey PRIMARY KEY (id),
    CONSTRAINT service_name_key UNIQUE (name),
    CONSTRAINT service_default_service_role_id_fkey FOREIGN KEY (default_role_id) REFERENCES data_inventory.owner_role (id),
    CONSTRAINT service_owner_id_fkey FOREIGN KEY (owner_id) REFERENCES data_inventory.owner (id),
    CONSTRAINT service_product_id_fkey FOREIGN KEY (product_id) REFERENCES data_inventory.product (id)
);

--;; Table: data_inventory.cluster
CREATE TABLE IF NOT EXISTS data_inventory.cluster (
    id SERIAL,
    name TEXT NOT NULL,
    CONSTRAINT cluster_pkey PRIMARY KEY (id),
    CONSTRAINT cluster_name_key UNIQUE (name)
);

--;; Table: data_inventory.cluster_service
CREATE TABLE IF NOT EXISTS data_inventory.cluster_service (
    cluster_id INTEGER NOT NULL,
    service_id INTEGER NOT NULL,
    CONSTRAINT cluster_service_pkey PRIMARY KEY (cluster_id, service_id),
    CONSTRAINT cluster_service_cluster_id_fkey FOREIGN KEY (cluster_id) REFERENCES data_inventory.cluster (id),
    CONSTRAINT cluster_service_service_id_fkey FOREIGN KEY (service_id) REFERENCES data_inventory.service (id)
);

--;; Table: data_inventory.data_category_group
CREATE TABLE IF NOT EXISTS data_inventory.data_category_group (
    id SERIAL,
    name TEXT NOT NULL,
    description TEXT,
    CONSTRAINT data_category_group_pkey PRIMARY KEY (id),
    CONSTRAINT data_category_group_name_key UNIQUE (name)
);

--;; Table: data_inventory.data_source
CREATE TABLE IF NOT EXISTS data_inventory.data_source (
    id INTEGER,
    name TEXT NOT NULL,
    description TEXT,
    has_eprivacy_content BOOLEAN,
    has_eprivacy_meta BOOLEAN,
    rank INTEGER NOT NULL DEFAULT 65535,
    CONSTRAINT data_source_pkey PRIMARY KEY (id),
    CONSTRAINT data_source_name_key UNIQUE (name)
);

--;; Table: data_inventory.data_category
CREATE TABLE IF NOT EXISTS data_inventory.data_category (
    id SERIAL,
    name TEXT NOT NULL,
    long_name TEXT,
    description TEXT,
    is_identifier BOOLEAN,
    is_special BOOLEAN,
    default_datasource_id INTEGER,
    data_category_group_id INTEGER NOT NULL,
    rank INTEGER NOT NULL DEFAULT 65535,
    CONSTRAINT data_category_pkey PRIMARY KEY (id),
    CONSTRAINT data_category_name_key UNIQUE (name),
    CONSTRAINT data_category_data_category_group_id_fkey FOREIGN KEY (data_category_group_id) REFERENCES data_inventory.data_category_group (id),
    CONSTRAINT data_category_default_datasource_id_fkey FOREIGN KEY (default_datasource_id) REFERENCES data_inventory.data_source (id)
);

--;; Table: data_inventory.data_subject_category
CREATE TABLE IF NOT EXISTS data_inventory.data_subject_category (
    id INTEGER,
    name TEXT NOT NULL,
    description TEXT,
    CONSTRAINT data_subject_category_pkey PRIMARY KEY (id),
    CONSTRAINT data_subject_category_name_key UNIQUE (name)
);

--;; Table: data_inventory.data_subject_location
CREATE TABLE IF NOT EXISTS data_inventory.data_subject_location (
    id INTEGER,
    name TEXT NOT NULL,
    CONSTRAINT data_subject_location_pkey PRIMARY KEY (id),
    CONSTRAINT data_subject_location_name_key UNIQUE (name)
);

--;; Table: data_inventory.legal_basis
CREATE TABLE IF NOT EXISTS data_inventory.legal_basis (
    id INTEGER,
    name TEXT NOT NULL,
    article TEXT,
    is_consent BOOLEAN,
    source TEXT,
    CONSTRAINT legal_basis_pkey PRIMARY KEY (id),
    CONSTRAINT legal_basis_name_key UNIQUE (name)
);

--;; Table: data_inventory.use_case_state
CREATE TABLE IF NOT EXISTS data_inventory.use_case_state (
    id INTEGER,
    name TEXT NOT NULL,
    CONSTRAINT use_case_state_pkey PRIMARY KEY (id),
    CONSTRAINT use_case_state_name_key UNIQUE (name)
);

--;; Table: data_inventory.purpose_category
CREATE TABLE IF NOT EXISTS data_inventory.purpose_category (
    id SERIAL,
    name TEXT NOT NULL,
    description TEXT,
    legal_basis_id INTEGER NOT NULL,
    default_role_id INTEGER,
    rank INTEGER NOT NULL DEFAULT 65535,
    CONSTRAINT purpose_category_pkey PRIMARY KEY (id),
    CONSTRAINT purpose_category_name_key UNIQUE (name),
    CONSTRAINT purpose_category_default_role_id_fkey FOREIGN KEY (default_role_id) REFERENCES data_inventory.owner_role (id),
    CONSTRAINT purpose_category_legal_basis_id_fkey FOREIGN KEY (legal_basis_id) REFERENCES data_inventory.legal_basis (id)
);

--;; Table: data_inventory.use_case
CREATE TABLE IF NOT EXISTS data_inventory.use_case (
    id SERIAL,
    name TEXT NOT NULL,
    product_id INTEGER NOT NULL,
    description TEXT,
    purpose_category_id INTEGER,
    owner_role_id INTEGER,
    use_case_state_id INTEGER,
    critical BOOLEAN NOT NULL DEFAULT FALSE,
    security_measure_url TEXT,
    CONSTRAINT use_case_pkey PRIMARY KEY (id),
    CONSTRAINT use_case_name_product_id_key UNIQUE (name, product_id),
    CONSTRAINT use_case_owner_role_id_fkey FOREIGN KEY (owner_role_id) REFERENCES data_inventory.owner_role (id),
    CONSTRAINT use_case_product_id_fkey FOREIGN KEY (product_id) REFERENCES data_inventory.product (id),
    CONSTRAINT use_case_purpose_category_id_fkey FOREIGN KEY (purpose_category_id) REFERENCES data_inventory.purpose_category (id),
    CONSTRAINT use_case_use_case_state_id_fkey FOREIGN KEY (use_case_state_id) REFERENCES data_inventory.use_case_state (id)
);

--;; Table: data_inventory.use_case_data_subject_category
CREATE TABLE IF NOT EXISTS data_inventory.use_case_data_subject_category (
    data_subject_category_id INTEGER NOT NULL,
    use_case_id INTEGER NOT NULL,
    location_id INTEGER NOT NULL DEFAULT 1,
    CONSTRAINT use_case_data_subject_category_pkey PRIMARY KEY (data_subject_category_id, use_case_id, location_id),
    CONSTRAINT use_case_data_subject_category_data_subject_category_id_fkey FOREIGN KEY (data_subject_category_id) REFERENCES data_inventory.data_subject_category (id),
    CONSTRAINT use_case_data_subject_category_location_id_fkey FOREIGN KEY (location_id) REFERENCES data_inventory.data_subject_location (id),
    CONSTRAINT use_case_data_subject_category_use_case_id_fkey FOREIGN KEY (use_case_id) REFERENCES data_inventory.use_case (id)
      ON DELETE CASCADE
);

--;; Table: data_inventory.data_use
CREATE TABLE IF NOT EXISTS data_inventory.data_use (
    id SERIAL,
    use_case_id INTEGER NOT NULL,
    data_category_id INTEGER NOT NULL,
    CONSTRAINT data_use_pkey PRIMARY KEY (id),
    CONSTRAINT data_use_use_case_id_data_category_id_key UNIQUE (use_case_id, data_category_id),
    CONSTRAINT data_use_data_category_id_fkey FOREIGN KEY (data_category_id) REFERENCES data_inventory.data_category (id),
    CONSTRAINT data_use_use_case_id_fkey FOREIGN KEY (use_case_id) REFERENCES data_inventory.use_case (id)
      ON DELETE CASCADE
);

--;; Table: data_inventory.data_use_cluster
CREATE TABLE IF NOT EXISTS data_inventory.data_use_cluster (
    data_use_id INTEGER NOT NULL,
    cluster_id INTEGER NOT NULL,
    owner_role_id INTEGER,
    data_retention_time TEXT,
    CONSTRAINT data_use_cluster_pkey PRIMARY KEY (data_use_id, cluster_id),
    CONSTRAINT data_use_cluster_cluster_id_fkey FOREIGN KEY (cluster_id) REFERENCES data_inventory.cluster (id),
    CONSTRAINT data_use_cluster_data_use_id_fkey FOREIGN KEY (data_use_id) REFERENCES data_inventory.data_use (id)
      ON DELETE CASCADE,
    CONSTRAINT data_use_cluster_owner_role_id_fkey FOREIGN KEY (owner_role_id) REFERENCES data_inventory.owner_role (id)
);

--;; Table: data_inventory.data_use_data_source
CREATE TABLE IF NOT EXISTS data_inventory.data_use_data_source (
    data_use_id INTEGER NOT NULL,
    data_source_id INTEGER NOT NULL,
    CONSTRAINT data_use_data_source_pkey PRIMARY KEY (data_use_id, data_source_id),
    CONSTRAINT data_use_data_source_data_source_id_fkey FOREIGN KEY (data_source_id) REFERENCES data_inventory.data_source (id),
    CONSTRAINT data_use_data_source_data_use_id_fkey FOREIGN KEY (data_use_id) REFERENCES data_inventory.data_use (id)
      ON DELETE CASCADE
);

--;; Table: data_inventory.processing_type
CREATE TABLE IF NOT EXISTS data_inventory.processing_type (
    id SERIAL,
    name TEXT NOT NULL,
    CONSTRAINT processing_type_pkey PRIMARY KEY (id),
    CONSTRAINT processing_type_name_key UNIQUE (name)
);

--;; Table: data_inventory.data_use_processing_type
CREATE TABLE IF NOT EXISTS data_inventory.data_use_processing_type (
    data_use_id INTEGER NOT NULL,
    processing_type_id INTEGER NOT NULL DEFAULT 5,
    CONSTRAINT data_use_processing_type_pkey PRIMARY KEY (data_use_id, processing_type_id),
    CONSTRAINT data_use_processing_type_data_use_id_fkey FOREIGN KEY (data_use_id) REFERENCES data_inventory.data_use (id)
      ON DELETE CASCADE,
    CONSTRAINT data_use_processing_type_processing_type_id_fkey FOREIGN KEY (processing_type_id) REFERENCES data_inventory.processing_type (id)
);

--;; Table: data_inventory.data_use_service
CREATE TABLE IF NOT EXISTS data_inventory.data_use_service (
    data_use_id INTEGER NOT NULL,
    service_id INTEGER NOT NULL,
    data_retention_time TEXT,
    owner_role_id INTEGER,
    CONSTRAINT data_use_service_pkey PRIMARY KEY (data_use_id, service_id),
    CONSTRAINT data_use_service_data_use_id_fkey FOREIGN KEY (data_use_id) REFERENCES data_inventory.data_use (id)
      ON DELETE CASCADE,
    CONSTRAINT data_use_service_owner_role_id_fkey FOREIGN KEY (owner_role_id) REFERENCES data_inventory.owner_role (id),
    CONSTRAINT data_use_service_service_id_fkey FOREIGN KEY (service_id) REFERENCES data_inventory.service (id)
);

--;; Table: data_inventory.dpia_state
CREATE TABLE IF NOT EXISTS data_inventory.dpia_state (
    id INTEGER,
    name TEXT NOT NULL,
    CONSTRAINT dpia_state_pkey PRIMARY KEY (id),
    CONSTRAINT dpia_state_name_key UNIQUE (name)
);

--;; Table: data_inventory.dpia
CREATE TABLE IF NOT EXISTS data_inventory.dpia (
    id SERIAL,
    product_id INTEGER NOT NULL,
    dpia_date TIMESTAMP WITHOUT TIME ZONE,
    url TEXT,
    dpia_state_id INTEGER NOT NULL,
    CONSTRAINT dpia_pkey PRIMARY KEY (id),
    CONSTRAINT dpia_dpia_state_id_fkey FOREIGN KEY (dpia_state_id) REFERENCES data_inventory.dpia_state (id),
    CONSTRAINT dpia_product_id_fkey FOREIGN KEY (product_id) REFERENCES data_inventory.product (id)
);

--;; Table: data_inventory.person
CREATE TABLE IF NOT EXISTS data_inventory.person (
    id SERIAL,
    first_name TEXT NOT NULL,
    last_name TEXT NOT NULL,
    confluence_link TEXT,
    email TEXT,
    tel TEXT,
    CONSTRAINT person_pkey PRIMARY KEY (id)
);

--;; Table: data_inventory.person_role
CREATE TABLE IF NOT EXISTS data_inventory.person_role (
    id INTEGER,
    name TEXT NOT NULL,
    CONSTRAINT person_role_pkey PRIMARY KEY (id),
    CONSTRAINT person_role_name_key UNIQUE (name)
);

--;; Table: data_inventory.product_contact
CREATE TABLE IF NOT EXISTS data_inventory.product_contact (
    product_id INTEGER NOT NULL,
    person_role_id INTEGER NOT NULL,
    person_id INTEGER NOT NULL,
    CONSTRAINT product_contact_pkey PRIMARY KEY (product_id, person_role_id, person_id),
    CONSTRAINT product_contact_person_id_fkey FOREIGN KEY (person_id) REFERENCES data_inventory.person (id),
    CONSTRAINT product_contact_person_role_id_fkey FOREIGN KEY (person_role_id) REFERENCES data_inventory.person_role (id),
    CONSTRAINT product_contact_product_id_fkey FOREIGN KEY (product_id) REFERENCES data_inventory.product (id)
);
